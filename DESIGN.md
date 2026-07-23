# Admin Web Design System

## Product Direction

The Admin Web is a compact operational workspace, not a marketing site.

- Full-screen application shell.
- Dense, calm, and easy to scan during repeated management tasks.
- Mostly black, white, and neutral gray with dark blue as the primary accent.
- Subtle borders and shadows; avoid decorative gradients and oversized surfaces.
- Consistent interaction, keyboard support, and permission-aware navigation.

## UI Technology

The design-system stack is:

```text
React 19 + Vite
Tailwind CSS 4 semantic tokens
shadcn-style source components
Base UI interaction primitives
Phosphor Icons
```

Rules:

- UI components are project-owned source files under `web/admin/src/components/ui`.
- Base UI owns accessible interaction behavior for components such as Popover and Combobox.
- Feature code imports project UI wrappers, never `@base-ui/react` directly.
- Use Base UI's `render` composition contract; do not use Radix `asChild` conventions.
- Do not introduce Radix UI, `cmdk`, or another competing primitive system.
- Use `@phosphor-icons/react` exclusively for interface icons.
- The project is not managed by a shadcn preset. Do not run destructive shadcn initialization or overwrite established components and tokens.

## Component Ownership

Use existing shared components before adding feature-specific markup.

```text
components/ui        visual and interaction primitives
components/layout    application shell and navigation composition
components/<feature> reusable feature presentation
pages                route-level orchestration
```

Shared component conventions:

- Keep the public API small and feature-independent.
- Style variants live in the shared component, not duplicated in consumers.
- Use `cn()` for conditional classes.
- Use semantic variants such as `default`, `secondary`, `outline`, `success`, `warning`, and `destructive`.
- Use `gap-*` for spacing and `size-*` for equal width and height.
- Use `truncate` for labels that must remain on one line.
- Do not hardcode colors when a semantic token exists.

## Design Tokens

Tokens live in:

```text
web/admin/src/styles/globals.css
```

Tailwind utilities must reference semantic tokens such as:

```text
background / foreground
card / card-foreground
popover / popover-foreground
primary / primary-foreground
secondary / secondary-foreground
muted / muted-foreground
accent / accent-foreground
destructive
success / success-muted
warning / warning-muted
border / input / ring
sidebar / sidebar-foreground / sidebar-primary
```

Primary dark blue is reserved for selected navigation, primary actions, focus rings, selected controls, and key highlights. Status colors communicate meaning and must not become decorative accents.

Dark mode must be implemented by changing semantic token values under `.dark`, not by scattering manual `dark:` color overrides through components.

## Typography

Keep typography compact and consistent.

- Page title: `text-2xl` to `text-3xl`, `font-semibold`.
- Section or card title: `text-sm` to `text-base`, `font-semibold`.
- Navigation: `text-[13px]`, normally `font-semibold`.
- Table and body: `text-[13px]` or `text-sm`.
- Metadata and helper text: `text-xs` or `text-[13px] text-muted-foreground`.
- Eyebrows and table headers may use uppercase tracking.

Do not use display-sized typography inside management screens.

## Density, Spacing, And Radius

Compact defaults:

- Page padding: `px-5 py-6`, increasing to `lg:px-9` where appropriate.
- Sidebar padding: `px-4 py-5`.
- Card padding: normally `p-4`.
- Section and grid gap: normally `gap-4`.
- Navigation item: `h-9`.
- Child navigation item: `h-8`.
- Control and button: normally `h-8` or `h-9`.
- Normal radius: `rounded-md` or `rounded-lg`.
- Card radius: `rounded-xl`.

Avoid excessive padding, large corner radii, and nested rounded containers.

## Application Shell

- The shell fills the viewport and reads as one continuous workspace.
- The desktop sidebar is sticky, `h-screen`, and separated by one right border.
- Main content owns the page scroll independently.
- Primary navigation has its own vertical scrolling region when necessary.
- Support, Settings, Log out, and account identity remain visible at the bottom of the sidebar.
- Do not wrap the entire sidebar and main content in floating cards.

## Sidebar Navigation

- Logo and product name stay at the top-left.
- Top-level leaves and selected child items use dark blue with white text.
- Categories expand and collapse without receiving a selected background themselves.
- Opening a category navigates to its first available child.
- Expanded children use connector lines to show hierarchy.
- Long labels truncate and never force the sidebar wider.
- Accounts groups Admins and Customers.
- Utility links use the same compact row treatment but remain visually separate from business navigation.
- Log out uses the destructive semantic color.
- Account identity shows initials, display name, and role at the bottom.

## Page Headers And Breadcrumbs

- Use the shared `PageHeader` component.
- Eyebrow, title, description, and actions use a clear vertical hierarchy.
- The page title owns its row.
- Primary actions appear below the title and description, aligned left.
- Create, detail, and edit routes use the shared semantic Breadcrumb instead of a large back button.
- Breadcrumb separators are sibling list items, hidden from assistive technology.
- The current page uses `aria-current="page"`.

## Cards And Data Blocks

- Use subtle borders and little or no shadow.
- Keep card padding compact.
- Do not place an entire page inside one decorative card unless it is genuinely one data surface.
- Metrics follow the pattern: short label, prominent value, compact trend Badge.
- Related identity and permission details may share one card with separated vertical sections.
- Card content must not force neighboring sections to stretch unnecessarily.

## Tables And Lists

- Header: uppercase, muted, approximately `text-[11px]`.
- Body: `text-[13px]`.
- Cell padding: approximately `px-3 py-3`.
- Use subtle row separators and avoid heavy grid lines.
- Growing records use server-side paging.
- Reuse `ListPagination`; do not implement paging controls per feature.
- Previous and next controls are icon-only with accessible labels.
- Rows per page is a numeric input limited to `1–100`.
- Do not render redundant “Showing X of Y” copy when pagination already communicates position.

## Permission Matrix

- Permission groups are rows and action metadata are columns.
- The matrix uses backend-provided group/action metadata; the frontend must not derive display labels by splitting permission codes.
- Defined and selected states use compact square controls.
- Undefined combinations use a visually distinct non-interactive state.
- Editable matrices provide one global bulk-selection control and one control per permission group.
- Read-only role details render the complete catalog when the viewer is authorized, checking only assigned permissions.

## Forms

- Create and edit workflows use full pages inside the admin shell.
- Dialogs are reserved for short confirmations, especially destructive actions.
- Labels are concise and visually associated with their controls.
- Field errors appear directly below the field.
- Form-level errors appear above the relevant action area.
- Invalid controls expose `aria-invalid`; errors cannot rely on color alone.
- Loading actions are disabled and include a visible progress indicator.
- Do not duplicate API validation messages when the backend already returns localized field details.

## Comboboxes And Option Selection

Use the shared Base UI Combobox for searchable option selection.

- Local catalog mode filters supplied in-memory options.
- Server search mode requests bounded search results and must not fetch every page into the browser.
- Server search remains debounced, cancellable, and protected from stale responses.
- Do not apply client filtering again to server-filtered results.
- Selected labels remain stable when the selected record is not in the current result set.
- Loading, empty, error, disabled, and invalid states are distinct and accessible.
- Arrow keys navigate, Enter selects, and Escape closes.

## Popovers And Date/Time

- Popovers use the shared Base UI wrapper with Portal, Positioner, and Popup.
- Popup width should match the trigger where appropriate using Base UI positioning variables.
- Popups must not be clipped by cards or the sticky sidebar.
- Escape and outside click close the popup; focus returns to the trigger.
- DateTimePicker displays browser-local date/time but its value contract remains an ISO 8601 UTC string or an empty string.
- Date/time filters must never emit locale-formatted, date-only, or timezone-free values to URL/API state.

## Buttons And Icons

- Use the shared Button component and its existing variants.
- Primary actions use dark blue.
- Secondary actions use outline or muted treatment.
- Destructive actions use red.
- Use icon-only buttons only when the meaning is unambiguous; always provide an accessible label.
- Icons inside buttons use a consistent component-owned size.
- Text and icons remain vertically centered with a compact gap.

## Badges

Use the shared Badge component instead of styled spans.

- `success`: enabled, active, completed, positive.
- `warning`: pending or risky but non-error.
- `secondary`: neutral metadata.
- `outline`: low-emphasis catalog metadata.
- `destructive`: error or destructive state.

Badges remain compact, single-line, and visually subordinate to primary data.

## Loading, Empty, And Error States

- Loading states preserve layout and clearly indicate progress.
- Empty states explain what is missing and offer at most one clear next action.
- Error messages are concise, localized, and actionable.
- Not-found pages provide return and logout paths.
- Overlay loading/error content must not become selectable Combobox items.

## Accessibility

- All interactive controls are keyboard reachable.
- Focus-visible treatment uses the semantic `ring` token.
- Icon-only controls have accessible labels.
- Inputs expose labels and validation state.
- Selected and expanded states use appropriate ARIA attributes.
- Color is never the only indicator of status, selection, or error.
- Dialog-like components require an accessible title even when visually hidden.

## Internationalization UX

- English and Vietnamese are supported.
- The language control lives in Settings, not as a floating control over every page.
- Real user-facing product text must use localization keys.
- API requests send the selected language through `Accept-Language`.
- Backend-localized validation details may be shown directly in the matching form fields.
- Temporary dashboard sample data may remain hardcoded until replaced by a real feature.

## Review Checklist

Before adding or changing UI, confirm:

- Existing shared component reused where possible.
- Base UI imported only inside `components/ui`.
- No Radix UI, `cmdk`, or `asChild` convention introduced.
- Semantic tokens used instead of raw colors.
- Compact typography, spacing, and radius preserved.
- Keyboard, focus, disabled, loading, empty, and error states handled.
- Long labels truncate safely.
- User-facing text is localizable.
- Layout works with the sticky sidebar and independently scrolling main content.
