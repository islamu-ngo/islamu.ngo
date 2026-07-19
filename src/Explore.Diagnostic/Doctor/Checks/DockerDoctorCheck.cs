// ABOUTME: Checks Docker and Compose CLI availability without starting containers.
// ABOUTME: Keeps local and first-party Compose diagnostics non-mutating by using version commands only.

using Explore.Diagnostic.Doctor.Infrastructure;

namespace Explore.Diagnostic.Doctor.Checks;

public sealed class DockerDoctorCheck(IDoctorProcessRunner processRunner) : IDoctorCheck
{
    public string Code => "tooling.docker";
    public DoctorCheckCategory Category => DoctorCheckCategory.Tooling;

    public async Task<DoctorCheckResult> RunAsync(CancellationToken cancellationToken)
    {
        var docker = await processRunner.RunAsync("docker", "--version", cancellationToken);
        if (docker.ExitCode != 0)
        {
            return DoctorCheckResult.Fail(
                Code,
                Category,
                "Docker CLI is unavailable, so Compose-based local operations cannot be diagnosed.",
                "Install Docker and verify `docker --version` before running the stack.",
                "docs/GETTING_STARTED.md",
                DoctorRedactor.Redact(string.IsNullOrWhiteSpace(docker.StandardError) ? docker.StandardOutput : docker.StandardError));
        }

        var compose = await processRunner.RunAsync("docker", "compose version", cancellationToken);
        if (compose.ExitCode != 0)
        {
            return DoctorCheckResult.Fail(
                Code,
                Category,
                "Docker is available but `docker compose` is unavailable.",
                "Install the Docker Compose plugin and verify `docker compose version`.",
                "docs/GETTING_STARTED.md",
                DoctorRedactor.Redact(string.IsNullOrWhiteSpace(compose.StandardError) ? compose.StandardOutput : compose.StandardError));
        }

        return DoctorCheckResult.Pass(
            Code,
            Category,
            "Docker and Docker Compose are available for local and first-party diagnostics.",
            "Use `docker compose config` manually for full Compose interpolation checks; doctor does not start containers.",
            "docs/OPERATIONS.md",
            DoctorRedactor.Redact($"{docker.StandardOutput.Trim()} | {compose.StandardOutput.Trim()}"));
    }
}
