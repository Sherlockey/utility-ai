using Godot;

using System;

public abstract partial class Log : PanelContainer
{
    [Export]
    protected VBoxContainer _messagesVBox;
    [Export]
    protected ScrollContainer _scrollContainer;
    [Export]
    protected PackedScene _logEntryScene;

    protected const int MaxMessages = 100;

    protected int _count = 0;

    public override void _Ready()
    {
        // Empty _messagesVBox;
        foreach (Node child in _messagesVBox.GetChildren())
        {
            _messagesVBox.RemoveChild(child);
            child.QueueFree();
        }
    }

    public void Write(string message, bool doPrependColons = true, bool doAppendPeriod = true)
    {
        if (doPrependColons)
        {
            message = ":: " + message;
        }
        if (doAppendPeriod)
        {
            message += ".";
        }
        Label entry = _logEntryScene.Instantiate<Label>();
        entry.Text = message;
        _count++;
        if (_count > MaxMessages)
        {
            Node firstChild = _messagesVBox.GetChild(0);
            firstChild?.Free();
            _count--;
        }
        _messagesVBox.AddChild(entry);
        UpdateScroll();
    }

    private async void UpdateScroll()
    {
        // TODO @Error this causes an error if the scene is removed from the tree before the timer goes off.
        // Low priority
        await ToSignal(GetTree().CreateTimer(0.01f, true, false, true), Timer.SignalName.Timeout);
        _scrollContainer.ScrollVertical = (int)_scrollContainer.GetVScrollBar().MaxValue + 1;
    }
}
