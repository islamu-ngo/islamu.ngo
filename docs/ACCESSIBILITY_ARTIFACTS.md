<!-- ABOUTME: Provides unpublished accessibility statement, assistive-technology evidence, and release-gate templates. -->
<!-- ABOUTME: Prevents accessibility aspirations from being published as verified conformance before release evidence exists. -->

# Accessibility Artifacts

> **Audience:** First-party operators | Contributors | AI agents
> **Status:** Active
> **Owner:** Frontend
> **Last Verified:** 2026-07-16
> **Source Anchors:** `docs/ACCESSIBILITY.md`, `docs/TESTING.md`, `tests/Event.Architecture.Tests/AccessibilityConventionTests.cs`, `tests/Explore.Blazor.Client.Tests/Accessibility/SharedComponentAccessibilityTests.cs`

This page is a release-readiness artifact set. Keep the statement, assistive-technology matrix, evidence summary, and release gate checklist here, but do not publish the template sections as current conformance evidence until the release owner fills in the release-specific values.

---

## Accessibility Statement Template

> **Publication Status:** Unreleased template. Replace this line with a release date before publishing a public accessibility statement.

The ISLAMU website is intended to provide an accessible experience for people with disabilities. Before publishing this statement, replace intended or template language with evidence from the deployed release.

### Conformance Status

We aim to conform to the Web Content Accessibility Guidelines (WCAG) 2.2, Level AA. These guidelines explain how to make web content more accessible for people with disabilities and more user-friendly for everyone.

### Measures To Verify Before Publication

- Automated accessibility checks and their release output
- WCAG 2.2 AA color contrast across every shipped theme
- Semantic landmarks and heading structure across implemented public routes
- Keyboard access for every interactive element and primary journey
- Screen-reader announcements for implemented dynamic content
- Focus behavior for dialogs, route changes, validation, and async updates
- RTL layout and content parity for Arabic
- Skip-to-content behavior
- Reduced-motion behavior through `prefers-reduced-motion`
- Forced-colors behavior where supported

### Known Limitations

- Some advanced form validation states are not announced to screen readers (`aria-invalid` not yet set by MudBlazor)
- Browser-level accessibility testing (Playwright + axe-core) not yet automated
- Not all data-loading pages announce completion to screen readers

### Feedback

We welcome feedback on the accessibility of the ISLAMU website. Configure and verify the following channels before publication:
- Email: Configure the public accessibility support address before publishing.
- Contact form: Configure the public accessibility contact URL before publishing.

Do not publish a response-time commitment until the responsible owner has approved and staffed it.

### Technical Specifications

This website relies on the following technologies:
- HTML5
- CSS3 (with logical properties for RTL support)
- JavaScript (ES modules)
- Blazor (.NET 10) with MudBlazor v9 component library
- WAI-ARIA 1.2

---

## Supported Assistive Technology Matrix

| AT | Browser | OS | Status | Notes |
|----|---------|----|--------|-------|
| NVDA 2024+ | Chrome | Windows | Unverified template | Primary desktop test target |
| NVDA 2024+ | Edge | Windows | Unverified template | Secondary Chromium target |
| JAWS 2024+ | Edge | Windows | Unverified template | Test when the release owner has access |
| VoiceOver | Safari | macOS | Unverified template | Primary Apple desktop target |
| VoiceOver | Safari | iOS | Unverified template | Primary Apple mobile target |
| TalkBack | Chrome | Android | Unverified template | Primary Android target |
| Narrator | Edge | Windows | Unverified template | Additional Windows target |

Keyboard-only, magnification, and forced-colors support must be recorded from the deployed release before these become public claims.

---

## Test Evidence Summary Template

The table below is a release evidence template. Replace the `Unverified template` entries with the actual release validation date and test output before using it as public evidence.

### Automated Test Results

| Test Suite | Pass | Fail | Skip | Date |
|------------|------|------|------|------|
| Architecture Convention Tests (a11y) | 8 | 0 | 0 | Unverified template |
| bUnit Component A11y Tests | 6 | 0 | 1 | Unverified template |

### Manual Test Results

| Flow | NVDA+Chrome | JAWS+Edge | VoiceOver+Safari | KB-only |
|------|-------------|-----------|------------------|---------|
| Read homepage and primary content | — | — | — | — |
| Browse and read published Blog content | — | — | — | — |
| Dialog open/close/focus | — | — | — | — |
| User settings | — | — | — | — |
| Any implemented authenticated journey | — | — | — | — |
| Dark mode | — | — | — | — |
| RTL (Arabic) | — | — | — | — |

Legend: ✅ Pass | ⚠️ Minor issues | ❌ Blocking | — Not yet tested

### axe-core Scan Results

> Deferred until Playwright E2E infrastructure is established.

---

## Release Gate Checklist

Before every public release, verify:

### Automated (CI enforced)
- [ ] All architecture convention tests pass (h1, landmarks, skip-link, live regions)
- [ ] All bUnit accessibility tests pass
- [ ] Build succeeds with 0 errors

### Manual (Pre-release)
- [ ] Keyboard-only: Complete every implemented primary public journey without a pointer
- [ ] NVDA + Chrome: Navigate homepage and published content; exercise an implemented dialog or dynamic region
- [ ] Focus management: Dialog open/close restores focus to trigger element
- [ ] Color contrast: Spot-check in both light and dark mode
- [ ] RTL: Switch to Arabic, verify layout mirrors correctly
- [ ] Reduced motion: Enable in OS settings, verify no animations
- [ ] Skip link: Tab to first element, verify skip-to-content link appears and works

### Documentation
- [ ] Accessibility statement is current
- [ ] Debt register reviewed — no critical items past target date
- [ ] ACCESSIBILITY.md updated with any new patterns
- [ ] Known limitations section in statement reflects current state
