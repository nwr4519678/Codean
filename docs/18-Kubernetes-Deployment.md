# 18 — Kubernetes Deployment

**Last updated:** 2026-08-03

We deploy on **AWS EKS** with **ArgoCD** for GitOps. The same chart is used in dev, staging, and prod with different `values.yaml` overlays.

## 1. Layout

```
infra/
└── helm/
    ├── platform/                 # umbrella chart for the whole platform
    │   ├── Chart.yaml
    │   ├── values.yaml
    │   ├── values-dev.yaml
    │   ├── values-staging.yaml
    │   ├── values-prod.yaml
    │   └── templates/
    │       ├── api/                # API Deployment + Service + Ingress
    │       ├── worker/             # Worker Deployment
    │       ├── judge/              # Judge Deployment (with KEDA on queue depth)
    │       ├── web/                 # Next.js (optional, usually on Vercel)
    │       ├── postgres/            # Bitnami chart reference
    │       ├── redis/               # Bitnami chart reference
    │       ├── elasticsearch/      # Operator-managed
    │       ├── hangfire-rbac/       # ServiceAccount + Role for job execution
    │       ├── ingress.yaml         # ALB ingress
    │       └── cert-issuer.yaml     # cert-manager
    └── scripts/
        ├── render-values.sh
        └── bootstrap-argocd.sh
```

## 2. API deployment (excerpt)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: platform-api
  namespace: platform
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate: { maxUnavailable: 0, maxSurge: 1 }
  selector:
    matchLabels: { app: platform-api }
  template:
    metadata:
      labels: { app: platform-api }
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "8080"
    spec:
      serviceAccountName: platform-api
      topologySpreadConstraints:
        - maxSkew: 1
          topologyKey: topology.kubernetes.io/zone
          whenUnsatisfiable: ScheduleAnyway
          labelSelector: { matchLabels: { app: platform-api } }
      containers:
        - name: api
          image: <ecr>/platform/api@sha256:...
          ports: [{ containerPort: 8080 }]
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: Production
            - name: ConnectionStrings__Postgres
              valueFrom: { secretKeyRef: { name: platform-secrets, key: postgres } }
            - name: Redis__Configuration
              valueFrom: { secretKeyRef: { name: platform-secrets, key: redis } }
            - name: Paymob__ApiKey
              valueFrom: { secretKeyRef: { name: platform-secrets, key: paymob-api } }
            - name: Paymob__HmacSecret
              valueFrom: { secretKeyRef: { name: platform-secrets, key: paymob-hmac } }
            - name: Jwt__PrivateKeyPem
              valueFrom: { secretKeyRef: { name: platform-jwt, key: private } }
            - name: Jwt__PublicKeyPem
              valueFrom: { secretKeyRef: { name: platform-jwt, key: public } }
          resources:
            requests: { cpu: "250m", memory: "512Mi" }
            limits:   { cpu: "1",    memory: "1Gi" }
          livenessProbe:
            httpGet: { path: /health/live, port: 8080 }
            initialDelaySeconds: 30
            periodSeconds: 30
            failureThreshold: 3
          readinessProbe:
            httpGet: { path: /health/ready, port: 8080 }
            initialDelaySeconds: 5
            periodSeconds: 10
            failureThreshold: 3
          startupProbe:
            httpGet: { path: /health/live, port: 8080 }
            failureThreshold: 30
            periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: platform-api
  namespace: platform
spec:
  selector: { app: platform-api }
  ports: [{ port: 80, targetPort: 8080 }]
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: platform-api
  namespace: platform
spec:
  scaleTargetRef: { apiVersion: apps/v1, kind: Deployment, name: platform-api }
  minReplicas: 3
  maxReplicas: 50
  metrics:
    - type: Resource
      resource: { name: cpu, target: { type: Utilization, averageUtilization: 60 } }
    - type: Pods
      pods: { metric: { name: http_requests_per_second }, target: { type: AverageValue, averageValue: "1k" } }
  behavior:
    scaleDown: { stabilizationWindowSeconds: 300 }
    scaleUp:   { stabilizationWindowSeconds: 30 }
```

## 3. Judge with KEDA autoscaling

```yaml
apiVersion: keda.sh/v1alpha1
kind: ScaledObject
metadata:
  name: platform-judge
  namespace: platform
spec:
  scaleTargetRef: { name: platform-judge }
  minReplicaCount: 2
  maxReplicaCount: 100
  cooldownPeriod: 60
  triggers:
    - type: postgres
      metadata:
        query: |
          SELECT 1 FROM hangfire.job
          WHERE statename='Enqueued' AND jobtype LIKE '%Judge%'
        targetQueryValue: "5"
        activationTargetQueryValue: "1"
        connectionFromEnv: HANGFIRE_PG
```

## 4. Ingress

ALB ingress with TLS termination via cert-manager. WAF rules applied via AWS WAFv2.

## 5. Pod disruption budget

```yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: platform-api
  namespace: platform
spec:
  minAvailable: 2
  selector: { matchLabels: { app: platform-api } }
```

## 6. Network policies

- API: egress to Postgres, Redis, ES, R2, Paymob; ingress from ALB only.
- Worker: same egress; no ingress.

## 7. HPA behavior

- API scales on CPU + custom Prometheus metric (RPS).
- Worker scales on Postgres job count.
- All use 5-min scale-down stabilization to avoid thrash.

## 8. Secrets

`platform-secrets`, `platform-jwt` are `kubernetes.io/dockerconfigjson` or `Opaque` secrets synced from AWS Secrets Manager by the **External Secrets Operator**.

## 9. Deploy workflow

```
PR merged → GitHub Actions → builds + pushes image to ECR →
  updates infra/helm/platform/values-prod.yaml (image tag) →
    commits to main → ArgoCD detects change → syncs cluster
```

Zero-downtime: `maxUnavailable: 0` + preStop hook that drains in-flight requests.
