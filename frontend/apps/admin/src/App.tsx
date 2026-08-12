import { FormEvent, createContext, useContext, useEffect, useState } from "react";
import {
  Activity,
  ArrowRight,
  BarChart3,
  Bell,
  BookOpen,
  Check,
  CircleDollarSign,
  Code2,
  CreditCard,
  Download,
  FileClock,
  Filter,
  GraduationCap,
  LayoutDashboard,
  LockKeyhole,
  LogOut,
  Menu,
  MoreHorizontal,
  Search,
  Server,
  Settings,
  ShieldCheck,
  TrendingUp,
  UserPlus,
  Users,
  Video,
  WalletCards,
  X,
} from "lucide-react";
import { Link, Navigate, NavLink, Route, Routes, useLocation, useNavigate } from "react-router-dom";

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5294";
const ACCESS_TOKEN_KEY = "codean_admin_access_token";
const REFRESH_TOKEN_KEY = "codean_admin_refresh_token";

type AdminUser = { userId: number; email: string; fullName: string; role: string };
type LoginResponse = AdminUser & { accessToken: string; refreshToken: string };
type AuthState = { user: AdminUser | null; checking: boolean; login: (email: string, password: string) => Promise<void>; logout: () => void };

const AuthContext = createContext<AuthState | null>(null);

async function apiRequest<T>(path: string, init?: RequestInit): Promise<T> {
  const token = localStorage.getItem(ACCESS_TOKEN_KEY);
  const response = await fetch(`${API_URL}${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...(token ? { Authorization: `Bearer ${token}` } : {}), ...init?.headers },
  });
  if (!response.ok) {
    const problem = await response.json().catch(() => null) as { detail?: string; title?: string } | null;
    throw new Error(problem?.detail ?? problem?.title ?? `Request failed (${response.status})`);
  }
  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AdminUser | null>(null);
  const [checking, setChecking] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    if (!token) { setChecking(false); return; }
    apiRequest<AdminUser & { emailConfirmed: boolean; createdAt: string }>("/api/auth/me")
      .then((current) => {
        if (current.role.toLowerCase() !== "admin") throw new Error("Admin access required.");
        setUser(current);
      })
      .catch(() => {
        localStorage.removeItem(ACCESS_TOKEN_KEY);
        localStorage.removeItem(REFRESH_TOKEN_KEY);
      })
      .finally(() => setChecking(false));
  }, []);

  const login = async (email: string, password: string) => {
    const result = await apiRequest<LoginResponse>("/api/auth/login", { method: "POST", body: JSON.stringify({ email, password, rememberMe: true }) });
    if (result.role.toLowerCase() !== "admin") throw new Error("This console is restricted to platform administrators.");
    localStorage.setItem(ACCESS_TOKEN_KEY, result.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, result.refreshToken);
    setUser(result);
  };

  const logout = () => {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    setUser(null);
  };

  return <AuthContext.Provider value={{ user, checking, login, logout }}>{children}</AuthContext.Provider>;
}

function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("AuthProvider is missing.");
  return context;
}

function Protected({ children }: { children: React.ReactNode }) {
  const { user, checking } = useAuth();
  const location = useLocation();
  if (checking) return <div className="boot-screen"><span /><p>Verifying administrator session...</p></div>;
  if (!user) return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  return children;
}

function LoginPage() {
  const { user, login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  if (user) return <Navigate to="/" replace />;
  const submit = async (event: FormEvent) => {
    event.preventDefault(); setError(""); setLoading(true);
    try { await login(email, password); navigate("/"); } catch (err) { setError(err instanceof Error ? err.message : "Unable to sign in."); } finally { setLoading(false); }
  };
  return <main className="login-page"><section className="login-brand"><Link to="http://localhost:5173" className="brand"><span><Code2 size={21} /></span>CODEAN</Link><div><p className="kicker">Restricted system</p><h1>Platform administration, separated by design.</h1><p>Manage identities, access, catalog governance, subscriptions, and security history from a dedicated console.</p></div><footer><ShieldCheck size={16} /> Protected by role-based access control</footer></section><section className="login-form-wrap"><form onSubmit={submit}><span className="lock-mark"><LockKeyhole size={22} /></span><p className="kicker">Administrator access</p><h2>Sign in to the console</h2><p>Use an account with the Admin role.</p>{error && <div className="error-message">{error}</div>}<label><span>Email address</span><input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoComplete="username" placeholder="admin@platform.com" /></label><label><span>Password</span><input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required autoComplete="current-password" placeholder="••••••••••••" /></label><button disabled={loading}>{loading ? "Verifying..." : "Sign in securely"}<ArrowRight size={17} /></button><small>Development: admin@platform.com / AdminPassword123!</small></form></section></main>;
}

const navItems = [
  ["/", "Overview", LayoutDashboard], ["/users", "Users", Users], ["/courses", "Courses", BookOpen],
  ["/subscriptions", "Subscriptions", WalletCards], ["/audit-logs", "Audit logs", FileClock], ["/settings", "Settings", Settings],
] as const;

function ConsoleLayout() {
  const { user, logout } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  return <div className="console"><aside className={menuOpen ? "console-nav open" : "console-nav"}><header><Link to="/" className="brand"><span><Code2 size={20} /></span>CODEAN <em>ADMIN</em></Link><button onClick={() => setMenuOpen(false)}><X size={19} /></button></header><p className="nav-label">Operations</p><nav>{navItems.map(([to, label, Icon]) => <NavLink end={to === "/"} to={to} onClick={() => setMenuOpen(false)} className={({ isActive }) => isActive ? "active" : ""} key={to}><Icon size={18} />{label}</NavLink>)}</nav><footer><div><span className="admin-avatar">{user?.fullName.split(" ").map((part) => part[0]).join("").slice(0, 2)}</span><p><strong>{user?.fullName}</strong><small>{user?.email}</small></p></div><button onClick={logout} aria-label="Sign out"><LogOut size={18} /></button></footer></aside>{menuOpen && <button className="backdrop" onClick={() => setMenuOpen(false)} />}<section className="console-workspace"><header className="console-header"><button className="mobile-menu" onClick={() => setMenuOpen(true)}><Menu size={20} /></button><div><ShieldCheck size={15} /><span>Authenticated Admin session</span></div><div><button><Bell size={18} /><i /></button><span className="admin-avatar">SA</span></div></header><main><Routes><Route index element={<Dashboard />} /><Route path="users" element={<UsersPage />} /><Route path="courses" element={<CoursesPage />} /><Route path="subscriptions" element={<SubscriptionsPage />} /><Route path="audit-logs" element={<AuditLogsPage />} /><Route path="settings" element={<SettingsPage />} /><Route path="*" element={<Navigate to="/" replace />} /></Routes></main></section></div>;
}

function Header({ eyebrow, title, copy, action }: { eyebrow: string; title: string; copy: string; action?: React.ReactNode }) {
  return <div className="page-header"><div><p className="kicker">{eyebrow}</p><h1>{title}</h1><p>{copy}</p></div>{action}</div>;
}

function Metric({ icon: Icon, value, label, trend }: { icon: typeof Users; value: string; label: string; trend?: string }) {
  return <article className="metric"><span><Icon size={20} /></span><div><strong>{value}</strong><p>{label}</p>{trend && <small>{trend}</small>}</div></article>;
}

function Dashboard() {
  return <><Header eyebrow="Platform operations" title="Administration overview" copy="Monitor platform growth, learning activity, and service health." /><div className="metrics"><Metric icon={Users} value="18,420" label="Total users" trend="+12.4% this quarter" /><Metric icon={GraduationCap} value="1,284" label="Teachers" /><Metric icon={BookOpen} value="86" label="Published courses" /><Metric icon={CircleDollarSign} value="EGP 2.4m" label="Revenue" /></div><div className="dashboard-grid"><section className="panel chart-panel"><div className="panel-head"><div><p className="kicker">Growth</p><h2>Monthly enrollments</h2></div><span className="success-badge"><TrendingUp size={13} /> 18.2%</span></div><div className="bars">{[42,58,49,72,68,88,76,96,82,100,91,112].map((height, index) => <span style={{ height }} key={index} />)}</div><footer><span>Sep</span><span>Dec</span><span>Mar</span><span>Jun</span><span>Aug</span></footer></section><section className="panel services"><p className="kicker">System health</p><h2>All services operational</h2>{[[Server,"Core API","68 ms"],[Code2,"Judge workers","142 ms"],[CreditCard,"Payments","91 ms"],[Video,"Live provider","74 ms"]].map(([Icon,label,time]) => { const ServiceIcon = Icon as typeof Server; return <div key={label as string}><ServiceIcon size={17} /><span>{label as string}</span><strong>{time as string}</strong><i /></div>; })}</section></div></>;
}

type UserRow = { name: string; email: string; role: "Student" | "Teacher" | "Admin"; status: "Active" | "Suspended" };
const initialUsers: UserRow[] = [
  { name: "Nadia Hassan", email: "nadia@example.com", role: "Student", status: "Active" },
  { name: "Maya Hassan", email: "maya@example.com", role: "Teacher", status: "Active" },
  { name: "Omar Khalil", email: "omar@example.com", role: "Admin", status: "Active" },
  { name: "Lina Nasser", email: "lina@example.com", role: "Teacher", status: "Active" },
  { name: "Salma Tarek", email: "salma@example.com", role: "Student", status: "Suspended" },
];

function UsersPage() {
  const [users, setUsers] = useState(initialUsers);
  const [query, setQuery] = useState("");
  const [creating, setCreating] = useState(false);
  const filtered = users.filter((user) => `${user.name} ${user.email}`.toLowerCase().includes(query.toLowerCase()));
  return <><Header eyebrow="Identity and access" title="User management" copy="Create teacher accounts and manage roles and access." action={<button className="primary" onClick={() => setCreating(true)}><UserPlus size={16} /> Create teacher</button>} /><section className="panel table-panel"><div className="table-tools"><label><Search size={16} /><input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search users" /></label><button className="outline"><Filter size={15} /> Filters</button></div><table><thead><tr><th>User</th><th>Role</th><th>Status</th><th>Joined</th><th>Last active</th><th /></tr></thead><tbody>{filtered.map((user, index) => <tr key={user.email}><td><strong>{user.name}</strong><small>{user.email}</small></td><td><span className={`role-badge ${user.role.toLowerCase()}`}>{user.role}</span></td><td><span className={user.status === "Active" ? "status active" : "status suspended"}>{user.status}</span></td><td>Aug {index + 2}, 2026</td><td>{index + 1}h ago</td><td><button className="icon"><MoreHorizontal size={16} /></button></td></tr>)}</tbody></table></section>{creating && <CreateTeacherModal onClose={() => setCreating(false)} onCreated={(teacher) => setUsers([teacher, ...users])} />}</>;
}

function CreateTeacherModal({ onClose, onCreated }: { onClose: () => void; onCreated: (teacher: UserRow) => void }) {
  const [firstName, setFirstName] = useState(""); const [lastName, setLastName] = useState(""); const [email, setEmail] = useState(""); const [password, setPassword] = useState("");
  const [error, setError] = useState(""); const [loading, setLoading] = useState(false);
  const submit = async (event: FormEvent) => {
    event.preventDefault(); setError(""); setLoading(true);
    try {
      await apiRequest("/api/users", { method: "POST", body: JSON.stringify({ firstName, lastName, email, password, role: "Teacher" }) });
      onCreated({ name: `${firstName} ${lastName}`.trim(), email, role: "Teacher", status: "Active" }); onClose();
    } catch (err) { setError(err instanceof Error ? err.message : "Could not create teacher."); } finally { setLoading(false); }
  };
  return <div className="modal-backdrop" onMouseDown={onClose}><section className="modal" onMouseDown={(e) => e.stopPropagation()}><header><div><p className="kicker">Admin-only operation</p><h2>Create teacher account</h2></div><button className="icon" onClick={onClose}><X size={18} /></button></header><form onSubmit={submit}>{error && <div className="error-message">{error}</div>}<div className="two-fields"><label><span>First name</span><input value={firstName} onChange={(e) => setFirstName(e.target.value)} required /></label><label><span>Last name</span><input value={lastName} onChange={(e) => setLastName(e.target.value)} required /></label></div><label><span>Email address</span><input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required /></label><label><span>Temporary password</span><input type="password" value={password} onChange={(e) => setPassword(e.target.value)} minLength={8} required /><small>The teacher should change this after the first login.</small></label><div className="role-lock"><ShieldCheck size={17} /><div><strong>Teacher role</strong><span>The role is fixed for this account creation flow.</span></div><Check size={17} /></div><footer><button type="button" className="outline" onClick={onClose}>Cancel</button><button className="primary" disabled={loading}>{loading ? "Creating..." : "Create teacher"}</button></footer></form></section></div>;
}

const courseRows = [
  ["Modern React Engineering","Maya Hassan","Frontend","428","Published"], ["Python Foundations","Omar Khalil","Programming","372","Published"],
  ["Algorithms and Problem Solving","Lina Nasser","Computer Science","294","Published"], ["Production APIs with .NET","Youssef Ali","Backend","0","Needs review"],
];

function CoursesPage() { return <><Header eyebrow="Catalog governance" title="Course moderation" copy="Review publishing quality and catalog health." /><DataTable headers={["Course","Instructor","Category","Students","Status",""]} rows={courseRows.map((row) => [<><strong>{row[0]}</strong><small>38 lessons · 20h</small></>,row[1],row[2],row[3],<span className={row[4] === "Published" ? "status active" : "status review"}>{row[4]}</span>,<button className="outline">Review</button>])} /></>; }
function SubscriptionsPage() { return <><Header eyebrow="Commerce operations" title="Subscriptions" copy="Monitor recurring revenue and subscription health." /><div className="metrics"><Metric icon={CircleDollarSign} value="EGP 842k" label="Monthly recurring" trend="+9.4% this month" /><Metric icon={WalletCards} value="7,842" label="Active plans" /><Metric icon={TrendingUp} value="2.1%" label="Churn rate" /><Metric icon={CreditCard} value="98.7%" label="Payment success" /></div><DataTable headers={["Customer","Plan","Amount","Status","Renewal"]} rows={initialUsers.map((user,index) => [<strong>{user.name}</strong>,index%2?"Career":"Pro",`EGP ${index%2?"1,250":"2,100"}`,<span className="status active">Active</span>,`Sep ${12+index}, 2026`])} /></>; }
function AuditLogsPage() { const actions = ["User.RoleAssigned","User.Created","Course.Published","User.Suspended","Plan.Updated","Session.Revoked"]; return <><Header eyebrow="Security and compliance" title="Audit logs" copy="Immutable history for privileged platform actions." action={<button className="outline"><Download size={15} /> Export</button>} /><DataTable headers={["Timestamp","Actor","Action","Entity","IP address"]} rows={actions.map((action,index) => [`Aug 12, ${14-index}:24`,initialUsers[index%initialUsers.length].name,<code>{action}</code>,`${action.split(".")[0]} #${1024+index}`,`192.168.1.${21+index}`])} /></>; }
function SettingsPage() { return <><Header eyebrow="Platform configuration" title="Admin settings" copy="Control operational defaults and platform-wide policies." /><div className="settings-grid"><section className="panel"><h2>Registration</h2>{["Allow student self-registration","Require email verification","Allow teacher self-registration"].map((item,index) => <label className="toggle" key={item}><span>{item}{index===2&&<small>Disabled: teachers are Admin-created only.</small>}</span><input type="checkbox" defaultChecked={index<2} disabled={index===2} /></label>)}</section><section className="panel"><h2>Learning defaults</h2><label><span>Certificate completion threshold</span><input type="number" defaultValue={90} /></label><label><span>Default passing score</span><input type="number" defaultValue={70} /></label><button className="primary">Save configuration</button></section><section className="panel"><h2>Maintenance</h2><div className="setting-row"><Activity size={18} /><span><strong>Maintenance mode</strong><small>Temporarily disable learner access.</small></span><button className="outline">Configure</button></div><div className="setting-row"><BarChart3 size={18} /><span><strong>Cache management</strong><small>Refresh catalog and analytics caches.</small></span><button className="outline">Clear cache</button></div></section></div></>; }

function DataTable({ headers, rows }: { headers: string[]; rows: React.ReactNode[][] }) { return <section className="panel table-panel"><table><thead><tr>{headers.map((header,index) => <th key={`${header}-${index}`}>{header}</th>)}</tr></thead><tbody>{rows.map((row,rowIndex) => <tr key={rowIndex}>{row.map((cell,index) => <td key={index}>{cell}</td>)}</tr>)}</tbody></table></section>; }

export default function App() {
  return <AuthProvider><Routes><Route path="/login" element={<LoginPage />} /><Route path="/*" element={<Protected><ConsoleLayout /></Protected>} /></Routes></AuthProvider>;
}
