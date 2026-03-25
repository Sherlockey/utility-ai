using Godot;

using System;

public partial class MessageLog : PanelContainer
{
    [Export]
    private VBoxContainer _messagesVBox;
    [Export]
    private ScrollContainer _scrollContainer;
    [Export]
    private PackedScene _messageLogEntryScene;

    private static MessageLog s_messageLog = null;

    private const int MAX_MESSAGES = 100;

    private int _count = 0;

    public MessageLog()
    {
        s_messageLog = this;
    }

    public override void _Ready()
    {
        // Empty _messagesVBox;
        foreach (Node child in _messagesVBox.GetChildren())
        {
            _messagesVBox.RemoveChild(child);
            child.QueueFree();
        }
    }

    public static MessageLog Get()
    {
        return s_messageLog;
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
        Label entry = _messageLogEntryScene.Instantiate<Label>();
        entry.Text = message;
        _count++;
        if (_count > MAX_MESSAGES)
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
        // TODO this causes an error if the scene is removed from the tree before the timer goes off.
        // Low priority
        await ToSignal(GetTree().CreateTimer(0.05f), Timer.SignalName.Timeout);
        _scrollContainer.ScrollVertical = (int)_scrollContainer.GetVScrollBar().MaxValue + 1;
    }
}
