# Blazor complex object binding demo

This project is a small demo showcasing an approach to data binding in Blazor using complex objects.

## Motivation

Imagine you have a non-trivial data model. It's an object that may have nested objects (including collections, which may
also contain other objects). This isn't an uncommon situation.

You want to make a reusable component to which you can bind your model and have it control the various values of the
model's properties. You want to avoid having to bind each individual property every time you use the component.

Ideally, you could simply use something like `@bind-Value="_model""`. Supporting `@bind-{PARAMETER}` also means the
component *should* properly support letting you define your own `@bind-{PARAMETER}:get` and `@bind-{PARAMETER}:set`
bindings, as well as `@bind-{PARAMETER}:after` if you don't care how the data is bound, but still want to do something
whenever it changes.

## How this demo works

### Data flow

A parent component/page binds a value to a child component. When this component does something that should change this
value, the component notifies the parent that the value should be changed. The parent then updates the value and
re-renders.

That's how `[Parameter]` and `EventCallback<T>` work. A component __should not__ update a parameter directly by using
its setter. Instead, it should notify the parent component/page using `EventCallback<T>.InvokeAsync(newValue)`.

### Objects

In our case, we want to bind a complex object, and be able to deal with collections. However, because of how data
binding works, we can only notify the parent that the object itself has changed.

While we can't modify the `[Parameter]` object directly, we can modify its properties just fine. However, when we
do this, the rest of the component tree has no idea we did this, and there's no way for us to notify the tree that
we changed a specific _property_.

What we _can_ do is notify the parent that the object itself was modified. When we do this, Blazor will replace the
`[Parameter]` with itself, and will re-render any component that depends on the same object.

Example:

_SomeObject.cs_
```csharp
public class SomeObject
{
    public string SomeProperty { get; set; }    
}
```

_SomeObjectComponent.cs_
```razor
<input @bind:get="Value.Property" @bind:set="HandlePropertyChangedAsync" />

@code {
    [Parameter]
    public SomeObject Value { get; set; }
    
    [Parameter]
    public EventCallback<SomeObject> ValueChanged { get; set; }

    private async Task HandlePropertyChangedAsync(T newValue)
    {
        Value.SomeProperty = newValue;
        await ValueChanged.InvokeAsync(Value);
    }
}
```

We're passing the `[Parameter]` object itself as an argument when calling `ValueChanged.InvokeAsync(...)`.

### Event bubbling

Child components that deal with nested objects will need to notify their parents whenever a change occurs. Their parents
will need to notify _their_ parent, and so on. Otherwise, the top-level parent has no way of knowing that its children
were changed.

Consider the following code:

```csharp
public class Foo
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Bar
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Foo foo { get; set; }
}
```

If you bind a `Bar` model to a component, the component has no way of knowing whether another component depends on the
model. If a child component changes the `Foo` portion of the model, the `Bar` component needs to be notified, and it
needs to notify its parent so that the parent can properly re-render components that also depend on the model.

This event bubbling looks like this:

_FooComponent.razor_
```razor
<input @bind:get="Foo.Name" @bind:set="HandleNameChangedAsync" />

@code {
    [Parameter]
    public Foo Foo { get; set; }
    
    [Parameter]
    public EventCallback<Foo> FooChanged { get; set; }
    
    // Foo changes bubble up to parent component.
    private async Task HandleNameChangedAsync(string name)
    {
        Foo.Name = name;
        await FooChanged.InvokeAsync(Foo);
    }
}
```

_BarComponent.razor_
```razor
<input @bind:get="Bar.Name" @bind:set="HandleNameChangedAsync" />
<FooComponent @bind-Foo:get="Bar.Foo" @bind-Foo:set="HandleFooChangedAsync" />

@code {
    [Parameter]
    public Bar Bar { get; set; }
    
    [Parameter]
    public EventCallback<Bar> BarChanged { get; set; }
    
    // Bar changes bubble up to parent component.
    private async Task HandleNameChangedAsync(string name)
    {
        Bar.Name = name;
        await BarChanged.InvokeAsync(Bar);
    }
        
    // When Foo changes, Bar must notify its parent that a change was detected.
    private async Task HandleFooChangedAsync(Foo foo) =>
        await BarChanged.InvokeAsync(Bar);
}
```

_Page.razor_
```razor
<BarComponent @bind-Bar="_model" />

@code {
    private Bar _model = new Bar(1, "Bar", new Foo(2, "Foo"));
}
```

### First-class support for two-way data binding

Because this works using the standard two-way data binding syntax (2 `[Parameter]`s, 1 value, 1 `EventCallback`), we're
able to re-use these individual components. We're also able to override how their change event handling works by
defining our own `@bind-{PARAMETER}:get/set` pair.

### Working with references

Since changes bubble up the component tree, anything that depends on our model in the tree is correctly re-rendered.
Anything _outside_ our component might need to be notified manually, however. In most cases it shouldn't matter (since
C# is an object-oriented language that deals with references), but it's worth keeping in mind if you're passing around
_properties_, since _those_ might be passed by value, depending on their type.