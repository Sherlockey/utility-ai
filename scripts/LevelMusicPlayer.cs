using Godot;

using System;

public partial class LevelMusicPlayer : Node
{
    [Export]
    private AudioStreamPlayer _audioStreamPlayer;
    [Export]
    private AudioStream[] _audioStreams;
    [Export]
    private uint _index = 0;

    public override void _Ready()
    {
        _audioStreamPlayer.Finished += OnAudioStreamPlayerFinished;

        if (_index < _audioStreams.Length)
        {
            _audioStreamPlayer.Stream = _audioStreams[_index];
            _audioStreamPlayer.Play();
        }
    }

    private void OnAudioStreamPlayerFinished()
    {
        _index += 1;
        if (_index >= _audioStreams.Length)
        {
            _index = 0;
        }

        if (_index < _audioStreams.Length)
        {
            _audioStreamPlayer.Stream = _audioStreams[_index];
            _audioStreamPlayer.Play();
        }
    }
}
