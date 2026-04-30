using Godot;

using System;

public partial class StatusDisplay : PanelContainer
{
    [Export]
    private Label _nameLabel;
    [Export]
    private TextureRect _portrait;
    [Export]
    private Label _levelLabel;
    [Export]
    private Label _experienceLabel;
    [Export]
    private Label _healthLabel;
    [Export]
    private Label _attackLabel;
    [Export]
    private Label _speedLabel;
    [Export]
    private Label _movementLabel;
    [Export]
    private Label _accuracyLabel;
    [Export]
    private Label _evasionLabel;

    // name should be <= 8 characters long for proper alignment
    public void Update(StringName name, Texture2D portraitTexture, string level, string experience,
        string currentHealth, string maxHealth, string attack, string speed, string movement, string accuracy,
        string evasion)
    {
        _nameLabel.Text = name;
        _portrait.Texture = portraitTexture;
        _levelLabel.Text = "LV:" + level.PadZeros(2);
        _experienceLabel.Text = "XP:" + experience.PadZeros(2);
        _healthLabel.Text = "HP:" + currentHealth.PadZeros(3) + "/" + maxHealth.PadZeros(3);
        _attackLabel.Text = "ATK:" + attack.PadZeros(2);
        _speedLabel.Text = "SPD:" + speed.PadZeros(2);
        _movementLabel.Text = "MOV:" + movement.PadZeros(2);
        _accuracyLabel.Text = "ACC:" + accuracy.PadZeros(2);
        _evasionLabel.Text = "EVA:" + evasion.PadZeros(2);
    }
}
