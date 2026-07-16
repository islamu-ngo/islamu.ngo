<!-- ABOUTME: Responsible vulnerability reporting policy for the ISLAMU website and repository. -->
<!-- ABOUTME: Defines safe research boundaries without unsupported response-time promises. -->

# Security Policy

ISLAMU welcomes responsible reports that help protect the website, its contributors, and
its users.

## Report A Vulnerability

Email [contact@openislamu.org](mailto:contact@openislamu.org) with:

- the affected URL, component, or repository path;
- the impact and conditions required to reproduce the issue;
- minimal reproduction steps or a proof of concept that does not expose user data;
- any suggested remediation; and
- a safe way to contact you for follow-up.

Do not include secrets, access tokens, personal data, or data obtained from other users.
Do not open a public issue for an undisclosed vulnerability.

## Safe Research Boundaries

- Stop testing if you encounter personal data, credentials, or access beyond your own account.
- Do not modify, delete, download, or retain data that is not yours.
- Do not use denial of service, spam, social engineering, physical attacks, or destructive payloads.
- Do not run broad automated scans against ISLAMU infrastructure without prior written permission.
- Do not test third-party services through ISLAMU unless that provider has separately authorized it.
- Allow the maintainers a reasonable opportunity to investigate before public disclosure.

This policy is an invitation to coordinate in good faith; it does not create a bug-bounty
program, response-time guarantee, or legal agreement.

## Scope

Reports are in scope when they demonstrate a concrete security impact in this repository or
an ISLAMU-operated deployment. Product-quality issues without a security impact belong in
[GitHub Issues](https://github.com/islamu-ngo/islamu.ngo/issues).

Claims that depend only on missing optional headers, software version banners, self-XSS,
physical access, or a victim-controlled man-in-the-middle position generally require a
demonstrated security impact to be actionable.

## Disclosure

The maintainers will assess reproducible reports, coordinate remediation and disclosure when
appropriate, and may credit reporters who request recognition. No fixed acknowledgement or
resolution timeline is promised until an accountable security response process is published.
