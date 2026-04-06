using Godot;

using System;

public partial class MessageLog : Log
{
    private static MessageLog s_messageLog = null;

    public MessageLog()
    {
        s_messageLog = this;
    }

    public static MessageLog Get()
    {
        return s_messageLog;
    }
}
