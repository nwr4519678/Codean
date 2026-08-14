# Production code execution

Code execution is intentionally a thin external integration. The API does not
host a compiler, execute student code, run a judge worker, or require a code
execution queue.

## Language mapping

| Editor language | Execution path |
| --- | --- |
| Python | OnlineCompiler.io `python-3.14` |
| C# | OnlineCompiler.io `dotnet-csharp-9` |
| JavaScript / Deno | OnlineCompiler.io `typescript-deno` |
| HTML | Sandboxed iframe preview |
| CSS | Sandboxed iframe preview |

HTML and CSS are preview-only languages. They must never be sent to the
external execution API. The preview must use an iframe with a sandbox policy,
no same-origin access, and no access to the platform token or parent DOM.

## Deployment configuration

Set these API environment variables on the host:

```text
Judge__BaseUrl=https://api.onlinecompiler.io/
Judge__ApiKey=<OnlineCompiler API key>
Judge__TimeoutSeconds=30
```

The API key is server-side only. Do not put it in Vite variables, browser code,
Git history, or challenge payloads. OnlineCompiler currently documents a
synchronous REST endpoint, C# support, 30-second execution limits, 512 MB
memory limits, and a free monthly request allowance. Confirm the provider's
current quota before opening the platform to a larger cohort.
