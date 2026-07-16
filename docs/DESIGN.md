<!-- ABOUTME: Product experience direction for the public ISLAMU website and web application. -->
<!-- ABOUTME: Defines a distinctive values-led atmosphere without speculative routes or decorative novelty. -->

# ISLAMU Experience Direction

> **Audience:** Product contributors | Designers | Frontend engineers | Content owners
> **Status:** Active
> **Owner:** Product | Design
> **Last Verified:** 2026-07-15
> **Source Anchors:** `docs/PROJECT.md`, `docs/DESIGN_SYSTEM.md`, `docs/ACCESSIBILITY.md`, `docs/LOCALIZATION.md`, `src/Explore.Blazor/`, `src/Explore.Blazor.Client/`

## North Star

The ISLAMU website should feel like a calm, credible, contemporary institution with editorial
depth. It is not a generic charity template, an imported Event dashboard, or a visual-effects
showcase. Its distinctiveness comes from purpose, typography, content rhythm, multilingual
craft, useful interaction, and transparent next steps.

## Visitor Journey

1. **Orient:** understand what ISLAMU is and why it exists.
2. **Explore:** read trustworthy, well-structured editorial content.
3. **Participate:** use only real, owned destinations that explain what happens next.
4. **Return:** discover current content and services without manipulation or noisy engagement patterns.

## Visual Principles

- Use generous editorial spacing, clear hierarchy, and deliberate pacing.
- Prefer typography, content, and composition over decorative cards and gradients.
- Use color semantically and with WCAG 2.2 AA contrast.
- Preserve the existing three-tier token model and CSS layer architecture.
- Use logical CSS properties so Arabic layouts are native RTL compositions, not mirrored afterthoughts.
- Keep surfaces quiet and borders/shadows restrained; hierarchy must not depend on elevation alone.
- Use authentic, rights-cleared imagery only. Missing imagery is better than placeholder or unlicensed imagery.

## Interaction Principles

- Every animation explains state, hierarchy, or continuity.
- Respect `prefers-reduced-motion`; no essential meaning depends on motion.
- No hover-only controls, autoplay, gratuitous parallax, scroll hijacking, or cursor effects.
- Navigation uses semantic disclosure controls, visible focus, Escape handling, and keyboard/touch parity.
- Loading, empty, failure, and offline states remain comprehensible without optional third parties.

## Content Principles

- Headlines state purpose rather than slogans unsupported by evidence.
- Claims name an owner and source; impact figures require a verification date.
- Calls to action describe the destination and consequence.
- English, French, and Arabic content is reviewed as content, not merely machine-translated strings.
- Planned capabilities stay in internal roadmaps until the destination is implemented.

## Required Quality Gates

- WCAG 2.2 AA, semantic landmarks, skip link, visible focus, 200% zoom, and screen-reader labels.
- English/French/Arabic parity and RTL review.
- Reduced-motion and keyboard-only review.
- Responsive behavior from small mobile viewports through large desktop layouts.
- Public pages remain useful when analytics and engagement services are unavailable.
- SEO metadata and structured data describe only implemented content types and routes.
- Consent and privacy decisions precede non-essential analytics or email capture.

`docs/DESIGN_SYSTEM.md` remains the technical implementation contract. This document owns the
public atmosphere and decision criteria, not component APIs.
