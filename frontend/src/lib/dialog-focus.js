/** Keep Tab navigation inside a modal drawer, including Shift+Tab at its first control.
 * @param {import('react').KeyboardEvent<HTMLDialogElement>} event
 */
export function trapDialogFocus(event) {
  if (event.key !== 'Tab') return;
  const controls = Array.from(event.currentTarget.querySelectorAll('a[href], button, input, select, textarea, [tabindex]'))
    .filter(node => node instanceof HTMLElement && node.tabIndex >= 0 && !node.matches(':disabled') && node.getClientRects().length > 0);
  const first = controls[0], last = controls[controls.length - 1];
  if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last?.focus(); }
  else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first?.focus(); }
}
