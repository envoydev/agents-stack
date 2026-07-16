# MVVM advanced mechanics

SKILL.md owns the day-to-day MVVM conventions; this file is the deeper mechanics loaded when you actually build these.

## Dependency properties vs ViewModel state

These solve different problems; do not confuse them.

- Register a `DependencyProperty` only on a **control** and only when the value must participate in
  the WPF property system - styling, animation, templating, inheritance, or being set from XAML.
  Give it a default and metadata through `PropertyMetadata`; coerce or reject values with
  `CoerceValueCallback` / `ValidateValueCallback` when an invariant must hold.
- ViewModel state is **never** a `DependencyProperty`. It is an `[ObservableProperty]` raising
  `INotifyPropertyChanged`. A ViewModel that inherits `DependencyObject` has the architecture
  inverted.

## Attached properties

- Use an attached property to bolt behavior or data onto an existing control without subclassing
  (`Grid.Row`, `Validation.HasError`, your own `behaviors:Focus.IsFocused`).
- Each is a `DependencyProperty.RegisterAttached` plus a static `GetX` / `SetX` pair.
- Any side effect (subscribing an event, mutating the visual tree) lives in the property-changed
  callback and is undone symmetrically when the value clears or the element unloads. An attached
  property that subscribes without unsubscribing is a leak.

## Event subscriptions and weak events

- Subscribe and unsubscribe symmetrically - a handler wired in `Loaded` or `OnAttached` is removed in
  `Unloaded` or `OnDetaching`. The leak appears when the event source outlives the subscriber: a
  ViewModel or control handling a static, application-lifetime, or singleton-service event stays rooted
  by that subscription and never collects.
- Break the strong reference in that case - subscribe through `WeakEventManager<TSource, TEventArgs>`
  (or, for ViewModel-to-ViewModel notifications, the toolkit's `WeakReferenceMessenger`) so the handler
  does not keep the subscriber alive - or guarantee the symmetric unsubscribe on teardown. Prefer
  explicit unsubscribe where the lifetimes are clear; reach for the weak-event pattern when the source's
  lifetime is not yours to control.

## Validation lives on the ViewModel

- Implement `INotifyDataErrorInfo` on the ViewModel. The toolkit's `ObservableValidator` base plus
  data-annotation attributes (`[Required]`, `[Range]`, custom `ValidationAttribute`) gives it for
  free; call `ValidateProperty` / `ValidateAllProperties` to drive it. `IDataErrorInfo` is the
  legacy interface - touch it only to extend a screen already built on it.
- Validation logic is C# on the ViewModel, not `ValidationRule` subclasses declared in XAML - XAML
  rules cannot be unit-tested and entangle view and logic.
- Surface errors with a `Validation.ErrorTemplate` on the control. Never concatenate error strings
  into the bound value itself.
- Validate on set for immediate feedback; revalidate the whole aggregate on submit so cross-field
  rules fire.
