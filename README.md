<!-- ABOUTME: Public entrypoint for the first-party ISLAMU website and web application. -->
<!-- ABOUTME: Explains current scope, contribution paths, imported provenance, and licensing. -->

# ISLAMU

This repository is the home of the first-party ISLAMU website and web application.
It is being built as ISLAMU's digital front door: a multilingual, accessible place to
explain the mission, publish trustworthy content, help people participate, and support
future ISLAMU services without turning the website into a catalogue of promises.

## Current Direction

The repository is being refactored from an imported event-platform foundation into a
single ISLAMU deployment. The product contract is:

- **Foundation:** a factual, Event-free ISLAMU website shell.
- **Publish:** editorial content, Blog, search/discovery metadata, and purposeful navigation.
- **Engage:** waitlists, newsletters, contact, and transactional email only when their
  complete data, consent, ownership, and operational flows are implemented.
- **Expand:** future programs and community services remain discovery work until they have
  an approved workstream and a real destination.

Planned or deferred capabilities are not published as working links or calls to action.
The implementation workstreams live under [`dev/active/`](dev/active/).

## Experience Principles

- Purpose before promotion.
- English, French, and Arabic experiences with first-class right-to-left support.
- WCAG 2.2 AA accessibility, keyboard and touch parity, and reduced-motion support.
- Calm, useful interaction rather than decorative effects.
- Fast and resilient public pages with privacy-respecting analytics and consent.
- Clear ownership and evidence for every public claim.

## Local Development

Start with [`docs/GETTING_STARTED.md`](docs/GETTING_STARTED.md). Architecture, operations,
security, and contribution guidance are indexed from [`docs/index.md`](docs/index.md).

## Contributing

- Use [GitHub Issues](https://github.com/islamu-ngo/islamu.ngo/issues) for reproducible bugs
  and scoped proposals.
- Read [`docs/CONTRIBUTING.md`](docs/CONTRIBUTING.md) before opening a pull request.
- Follow the [Code of Conduct](CODE_OF_CONDUCT.md) and repository contribution checks.

## Humanitarian Support

The humanitarian suffering of Palestinians requires sustained attention and action.
Those able to help can support the
[Palestine Red Crescent Society](https://www.palestinercs.org/en/Donation).

## Contact And Security

For general repository questions, open a
[GitHub issue](https://github.com/islamu-ngo/islamu.ngo/issues/new). Report suspected
security vulnerabilities privately using [`SECURITY-POLICY.md`](SECURITY-POLICY.md).

No public privacy-policy link is published yet. A policy will be added only after the
website's analytics, cookies, email capture, retention, and data-owner decisions are
verified and legally reviewed.

## Provenance

This repository contains a snapshot import of the ISLAMU Event platform:

- **Source repository:** [islamu-ngo/Event](https://github.com/islamu-ngo/Event), licensed
  under GNU AGPL v3.
- **Imported snapshot:** commit `5cfa996b2344969856946955270dd333e06fc61d`
  (`develop`, 2026-07-14), tracked files only.
- **License continuity:** the source and this repository use AGPL v3; the import preserves
  the original license terms without modification.
- **Divergence:** `islamu.ngo` is the single first-party ISLAMU deployment, not a supported
  self-hostable or white-label product. Intentional divergences are recorded in
  [`dev/active/init-import-islamu-event/`](dev/active/init-import-islamu-event/).

## License

This project is licensed under the terms of the [GNU Affero General Public License v3](LICENSE).
