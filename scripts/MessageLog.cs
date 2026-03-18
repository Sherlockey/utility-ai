using Godot;

using System;

public partial class MessageLog : PanelContainer
{
    [Export]
    private VBoxContainer _messagesVBox;
    [Export]
    private PackedScene _messageLogEntryScene;

    private static MessageLog s_messageLog = null;

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
        if (_count > 100)
        {
            Node firstChild = _messagesVBox.GetChild(0);
            firstChild?.QueueFree();
            _count--;
        }
        _messagesVBox.AddChild(entry);
    }
}
