# Product

## Register

product

## Users

Small business owners in Malaysia managing their own books. Primary context: seated at a desk during business hours, working on a standard monitor. They are not accountants by trade; they need to create invoices, log expenses, finalize exports, and glance at financial health without drowning in accounting jargon. The system also serves accountant and staff roles within the same company, but the owner is the decision-maker and most frequent user.

## Product Purpose

Fintrack is a Malaysia-ready e-invoice and expense management system built as a learning project for ASP.NET Core. The frontend (React + Vite) consumes a .NET 10 controller-based API with EF Core, Identity, and JWT auth. It covers invoicing, expense tracking, dashboard reporting, and simulated Malaysian e-invoice export (JSON/CSV). Success looks like: a single business owner can sign in, create and finalize invoices with line items and tax, record expenses, export data, and review monthly revenue vs. expense trends, all from one working surface.

## Brand Personality

Modern, fast, efficient. The interface should feel like a tool built by someone who respects the user's time. No clutter, no unnecessary steps, no decorative noise. Confidence without coldness. The kind of product that feels obviously well-made the moment you open it.

## Anti-references

- Generic SaaS dashboards with big gradient numbers, icon grids, and stock illustration heroes.
- Overly playful or consumer-app aesthetics (rounded everything, pastel confetti, emoji-heavy UX).
- Cluttered legacy accounting software that exposes every possible field on one screen.

## Design Principles

1. **Respect the operator's time.** Every screen should answer "what do I do next?" within two seconds. If the user has to hunt, the layout failed.
2. **Earned complexity.** Show the minimum viable surface first. Advanced controls, filters, and bulk actions reveal themselves when the user's workflow demands them, not before.
3. **Numbers are the product.** Financial figures, totals, and trends are the primary content. Typography, spacing, and contrast should be tuned so monetary values read instantly and unambiguously.
4. **Structure over decoration.** Hierarchy comes from spacing, weight, and position, not from gradients, glows, or ornamental borders. Every visual element must carry information or aid navigation.
5. **Learn by building real things.** This is a learning project. Keep patterns explicit, conventional, and traceable. Avoid clever abstractions that obscure how the system works.

## Accessibility & Inclusion

WCAG 2.1 AA as the baseline. Ensure sufficient contrast ratios on all text over dark surfaces (the current palette leans warm-on-dark, which can underperform at small sizes). Support keyboard navigation for all interactive elements. Respect `prefers-reduced-motion`. No information conveyed by color alone; pair color status indicators with text labels or icons.
