---
name: Fintrack
description: Malaysia-ready e-invoice and expense management for small business owners.
colors:
  fired-clay: "oklch(0.58 0.14 28)"
  fired-clay-deep: "oklch(0.52 0.14 28)"
  fired-clay-subtle: "oklch(0.58 0.14 28 / 0.08)"
  parchment-white: "oklch(0.975 0.006 75)"
  rice-paper: "oklch(0.99 0.004 75)"
  warm-linen: "oklch(0.965 0.007 75)"
  sandstone-inset: "oklch(0.955 0.008 75)"
  sandstone-hover: "oklch(0.945 0.01 75)"
  warm-graphite: "oklch(0.22 0.008 75)"
  umber-secondary: "oklch(0.42 0.008 75)"
  driftwood-tertiary: "oklch(0.58 0.006 75)"
  line-soft: "oklch(0.88 0.008 75)"
  line-strong: "oklch(0.78 0.012 75)"
  jade-confirm: "oklch(0.55 0.14 155)"
  alert-ember: "oklch(0.55 0.16 25)"
  cool-slate: "oklch(0.65 0.08 220)"
typography:
  page-title:
    fontFamily: "'Inter', 'Segoe UI', system-ui, sans-serif"
    fontSize: "1.728rem"
    fontWeight: 700
    lineHeight: 1.2
    letterSpacing: "-0.01em"
  heading:
    fontFamily: "'Inter', 'Segoe UI', system-ui, sans-serif"
    fontSize: "1.44rem"
    fontWeight: 700
    lineHeight: 1.3
    letterSpacing: "-0.01em"
  title:
    fontFamily: "'Inter', 'Segoe UI', system-ui, sans-serif"
    fontSize: "1.2rem"
    fontWeight: 600
    lineHeight: 1.4
    letterSpacing: "-0.01em"
  body:
    fontFamily: "'Inter', 'Segoe UI', system-ui, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 400
    lineHeight: 1.5
    letterSpacing: "normal"
  label:
    fontFamily: "'Inter', 'Segoe UI', system-ui, sans-serif"
    fontSize: "0.8125rem"
    fontWeight: 600
    lineHeight: 1.4
    letterSpacing: "0.06em"
  caption:
    fontFamily: "'Inter', 'Segoe UI', system-ui, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 400
    lineHeight: 1.4
    letterSpacing: "normal"
rounded:
  sm: "0.375rem"
  md: "0.5rem"
  lg: "0.75rem"
spacing:
  xs: "0.25rem"
  sm: "0.5rem"
  md: "0.75rem"
  lg: "1rem"
  xl: "1.5rem"
  2xl: "2rem"
  3xl: "3rem"
components:
  button-primary:
    backgroundColor: "{colors.fired-clay}"
    textColor: "{colors.rice-paper}"
    rounded: "{rounded.md}"
    padding: "0 1rem"
    height: "36px"
  button-primary-hover:
    backgroundColor: "{colors.fired-clay-deep}"
  button-secondary:
    backgroundColor: "{colors.rice-paper}"
    textColor: "{colors.warm-graphite}"
    rounded: "{rounded.md}"
    padding: "0 1rem"
    height: "36px"
  button-ghost:
    backgroundColor: "transparent"
    textColor: "{colors.umber-secondary}"
    rounded: "{rounded.md}"
    padding: "0 1rem"
    height: "36px"
  button-ghost-hover:
    backgroundColor: "{colors.sandstone-inset}"
    textColor: "{colors.warm-graphite}"
  button-danger:
    backgroundColor: "oklch(0.55 0.16 25 / 0.08)"
    textColor: "{colors.alert-ember}"
    rounded: "{rounded.md}"
    padding: "0 1rem"
    height: "36px"
  input-default:
    backgroundColor: "{colors.rice-paper}"
    textColor: "{colors.warm-graphite}"
    rounded: "{rounded.md}"
    padding: "0 0.75rem"
    height: "40px"
  card-default:
    backgroundColor: "{colors.rice-paper}"
    rounded: "{rounded.lg}"
    padding: "1.5rem"
  nav-item:
    backgroundColor: "transparent"
    textColor: "{colors.umber-secondary}"
    rounded: "{rounded.md}"
    padding: "0.5rem 0.75rem"
  nav-item-active:
    backgroundColor: "{colors.fired-clay-subtle}"
    textColor: "{colors.fired-clay}"
---

# Design System: Fintrack

## 1. Overview

**Creative North Star: "The Paper Office"**

Fintrack's interface channels the feeling of high-quality stationery: warm cream paper, crisp ink, no clutter. Every surface carries the quiet authority of a well-kept ledger. The system is built for a Malaysian small business owner seated at a desk during business hours, working on a standard monitor in a well-lit office. That scene demands a light mode with warm, low-chroma neutrals that read clearly under natural and fluorescent light.

The system explicitly rejects generic SaaS dashboards with big gradient numbers and icon grids; overly playful consumer-app aesthetics with rounded everything and pastel confetti; and cluttered legacy accounting software that exposes every possible field on one screen. If someone can guess the palette from the category name alone ("finance → navy + gold"), the design has failed. Fintrack breaks that reflex with warm stone neutrals and a single fired-clay accent that reads more like architecture than banking.

**Key Characteristics:**
- Light mode, warm-tinted neutrals throughout; never pure white, never pure black
- Single accent color (Fired Clay terracotta) used sparingly on primary actions and active states only
- Numbers as primary content: `tabular-nums`, high-weight, prominent positioning
- Structure through spacing and weight, never through gradients, glows, or ornamental borders
- Flat by default; shadows appear only as structural hints, never as decoration

## 2. Colors: The Paper Office Palette

A restrained palette of warm-tinted neutrals with a single fired-clay accent. Every neutral carries chroma 0.004–0.012 at hue 75 (amber undertone), producing surfaces that feel warm without ever reading as beige or tan.

### Primary
- **Fired Clay** (`oklch(0.58 0.14 28)`): The sole accent. Used exclusively on primary action buttons, active navigation state, and focus rings. Its rarity is the point.
- **Fired Clay Deep** (`oklch(0.52 0.14 28)`): Hover state of primary buttons only. Never used at rest.

### Neutral
- **Parchment White** (`oklch(0.975 0.006 75)`): Page background. The ambient canvas that everything sits on.
- **Rice Paper** (`oklch(0.99 0.004 75)`): Card and panel backgrounds. Slightly lighter than the page to create a tonal lift without shadow.
- **Warm Linen** (`oklch(0.965 0.007 75)`): Sidebar background. A half-step cooler than the page to separate chrome from content.
- **Sandstone Inset** (`oklch(0.955 0.008 75)`): Inset panels (line items, totals, trend chart wells). Reads as recessed.
- **Sandstone Hover** (`oklch(0.945 0.01 75)`): Interactive hover state for list items and ghost buttons.
- **Warm Graphite** (`oklch(0.22 0.008 75)`): Primary text. Headlines, monetary values, and navigation labels.
- **Umber Secondary** (`oklch(0.42 0.008 75)`): Body text and form labels. Comfortable reading density without competing with values.
- **Driftwood Tertiary** (`oklch(0.58 0.006 75)`): Captions, timestamps, section bar titles, and supporting metadata.
- **Line Soft** (`oklch(0.88 0.008 75)`): Default borders, dividers, and separators. Visible at rest, not loud.
- **Line Strong** (`oklch(0.78 0.012 75)`): Hovered borders on secondary buttons. Slightly more assertive.

### Tertiary
- **Jade Confirm** (`oklch(0.55 0.14 155)`): Success messages and confirmed states. Paired with background tint at 8% opacity.
- **Alert Ember** (`oklch(0.55 0.16 25)`): Error messages, danger buttons, and validation failures. Same opacity pattern.
- **Cool Slate** (`oklch(0.65 0.08 220)`): Expense trend bars only. Provides visual separation from the revenue accent without adding a new warm hue.

### Named Rules
**The One Voice Rule.** Fired Clay is the only saturated color at rest on any screen. Its presence means "act here." If a second saturated color appears at rest, the hierarchy breaks.

**The Warm Neutral Rule.** Every achromatic value carries chroma 0.004–0.012 at hue 75. Pure `#000` and `#fff` are prohibited. This is what makes surfaces feel like paper instead of screens.

## 3. Typography

**Body Font:** Inter (with 'Segoe UI', system-ui, sans-serif fallback)

**Character:** A single sans-serif family carries every role. Inter was chosen for its tabular numeral support (`font-variant-numeric: tabular-nums`), tight tracking at small sizes, and native availability on Windows via Segoe UI fallback. No display font. No serif. The interface is dense with financial data; a second typeface would add noise without meaning.

### Hierarchy
- **Page Title** (700, 1.728rem, line-height 1.2): Top-level page identifiers. Dashboard, Invoices, Expenses. One per view.
- **Heading** (700, 1.44rem, line-height 1.3): Section headings and stat values. The size that monetary figures render at in stat cells.
- **Title** (600, 1.2rem, line-height 1.4): Panel headings, invoice numbers in detail view. The workhorse heading.
- **Body** (400, 0.875rem, line-height 1.5): Form labels, descriptions, ledger item names. Max line length for prose: 65–75ch.
- **Label** (600, 0.8125rem, letter-spacing 0.06em, uppercase): Section bar titles, eyebrow labels, role tags. Always uppercase with tracked spacing. Used for categorical identification, never for reading.
- **Caption** (400, 0.75rem, line-height 1.4): Timestamps, metadata notes, detail notes. The smallest text in the system.

### Named Rules
**The Scale Ratio Rule.** The type scale follows a 1.2 ratio (minor third): 0.75 → 0.875 → 1 → 1.2 → 1.44 → 1.728. Fixed rem steps, not fluid. Product UIs don't benefit from viewport-scaled headings.

**The Tabular Rule.** Every monetary value renders with `font-variant-numeric: tabular-nums` so columns of numbers align vertically without layout shift. This is non-negotiable for financial data.

## 4. Elevation: The Flat Desk

Fintrack is flat by default. Depth is conveyed through tonal layering (page → card → inset) rather than shadows. Shadows exist but serve as structural hints, never decoration.

### Shadow Vocabulary
- **Shadow Small** (`0 1px 2px oklch(0.22 0.008 75 / 0.04)`): Default on cards and panels. Barely perceptible; its role is to separate card from page without visual weight.
- **Shadow Medium** (`0 2px 8px oklch(0.22 0.008 75 / 0.06)`): Hover elevation. Applied to buttons on hover to signal interactivity.
- **Shadow Large** (`0 4px 16px oklch(0.22 0.008 75 / 0.08)`): Auth card only. The sign-in card floats above the page as the single focal element. No other surface uses this level.

### Named Rules
**The Flat Desk Rule.** Surfaces are flat at rest. Shadow Small is so subtle it's more texture than elevation. If a shadow is visible enough to cast a "floating" impression, it's too heavy for this system. Exception: the auth card, which IS the page.

## 5. Components

Quiet and precise. Nothing calls attention to itself except the primary action. Every component uses the same border radius vocabulary, the same transition timing, and the same state progression.

### Buttons
- **Shape:** Gently curved edges (0.5rem / 8px radius), 36px fixed height, 0.8125rem / 13px text
- **Primary:** Fired Clay background with Rice Paper text. The only saturated element in the default view. Hover deepens to Fired Clay Deep with Shadow Small.
- **Secondary:** Rice Paper background with Warm Graphite text and Line Soft border. Hover strengthens border to Line Strong and shifts background to Sandstone Hover.
- **Ghost:** Transparent background with Umber Secondary text. Hover fills with Sandstone Inset. Used for tertiary actions (export format buttons, remove line item).
- **Danger:** Alert Ember at 8% opacity background, Alert Ember text. Used for delete actions only.
- **Disabled:** 50% opacity, `cursor: not-allowed`. No other visual change.
- **Transitions:** `background`, `border-color`, and `box-shadow` at 150ms ease-out. No transform animations.

### Cards / Containers
- **Panel:** Rice Paper background, Line Soft border (1px), 0.75rem radius, Space XL (1.5rem) padding, Shadow Small. The universal container.
- **Inset Panel:** Sandstone Inset background, Line Soft border, same radius. Used inside panels for line items and totals. Creates a recessed well.
- **Stat Cell:** Same treatment as panel but with tighter padding (Space LG vertical, Space XL horizontal). Values render at Heading size (1.44rem, 700 weight).
- **Nesting prohibition:** Panels never nest inside panels. Use inset panels for embedded content regions.

### Inputs / Fields
- **Style:** Rice Paper background, Line Soft border, 0.5rem radius, 40px height, 0.875rem text
- **Focus:** Border shifts to Fired Clay; 3px focus ring at Fired Clay Subtle (8% opacity). This is the only place the accent appears outside buttons and navigation.
- **Error:** Validation text in Alert Ember at Caption size (0.75rem) below the input.
- **Textarea:** Same styling, minimum height 120px, vertical resize enabled.
- **Label:** Body weight (500, 0.8125rem) in Umber Secondary, 4px gap to input.

### Navigation
- **Sidebar:** 260px fixed width, Warm Linen background, 1px Line Soft right border. Contains brand block, nav list, and account block (pushed to bottom with `margin-top: auto`).
- **Nav Item:** 0.875rem, 500 weight, Umber Secondary text. Rounded (0.5rem), 1px transparent border. Hover fills Sandstone Hover and shifts text to Warm Graphite.
- **Active State:** Fired Clay Subtle background (8% opacity), Fired Clay text, 600 weight. The accent tint is the only color on the sidebar at rest.
- **Transitions:** 150ms ease-out on background and color.

### Ledger Items
- **Style:** Full-width interactive rows, transparent background, 1px transparent border, 0.5rem radius. Left-aligned primary info, right-aligned monetary values.
- **Hover:** Sandstone Hover background fill.
- **Active/Selected:** Fired Clay Subtle background, Fired Clay border at 12% opacity.
- **Values:** 0.8125rem, 600 weight, `tabular-nums`. Always right-aligned in the meta column.

### Status Messages
- **Success:** Jade Confirm text on Jade Confirm at 8% opacity background, 1px Jade Confirm at 15% opacity border.
- **Error:** Alert Ember text on Alert Ember at 8% opacity background, matching border treatment.
- **Shape:** 0.5rem radius, Space MD/LG padding, 0.8125rem 500 weight text.

## 6. Do's and Don'ts

### Do:
- **Do** use `font-variant-numeric: tabular-nums` on every monetary value. Columns of numbers must align.
- **Do** tint every neutral toward hue 75 with chroma 0.004–0.012. Paper, not screen.
- **Do** keep Fired Clay under 10% of any screen's surface area. Its rarity signals importance.
- **Do** use tonal layering (page → card → inset) to convey depth before reaching for shadows.
- **Do** pair color status indicators with text labels. "Finalized" as text, not just a green dot.
- **Do** respect `prefers-reduced-motion` with a blanket `transition-duration: 0.01ms` override.
- **Do** keep section bar titles uppercase, tracked, and in Driftwood Tertiary. They are wayfinding, not content.
- **Do** use consistent 150ms ease-out timing on all interactive state transitions.

### Don't:
- **Don't** use `#000` or `#fff`. Every value in this system carries a warm tint. Pure extremes look clinical against the Paper Office palette.
- **Don't** use border-left or border-right greater than 1px as a colored accent stripe on cards, alerts, or list items. Prohibited.
- **Don't** apply `background-clip: text` with a gradient. Gradient text is banned.
- **Don't** use glassmorphism, backdrop-filter, or blur effects. This system is opaque paper, not frosted glass.
- **Don't** build hero-metric templates (big gradient number, small label, sparkline). This is the SaaS cliché that PRODUCT.md's anti-references explicitly reject.
- **Don't** create identical card grids with icon + heading + text repeated in a row. Vary the layout.
- **Don't** use display or serif fonts in UI labels, buttons, or data fields. Inter carries everything.
- **Don't** introduce a second saturated color at rest. Cool Slate appears only in trend chart bars; Jade Confirm and Alert Ember appear only in transient status messages. Fired Clay is the only permanent accent.
- **Don't** animate CSS layout properties (`width`, `height`, `top`, `left`). Transition `background`, `color`, `border-color`, `box-shadow`, `opacity`, and `transform` only.
- **Don't** use em dashes in UI copy. Use commas, colons, semicolons, periods, or parentheses.
- **Don't** nest cards inside cards. Use inset panels for embedded content.
- **Don't** apply the navy+gold, dark-blue+teal, or any other category-reflex finance palette. The warm neutral + terracotta combination was chosen specifically to break that training-data pattern.
