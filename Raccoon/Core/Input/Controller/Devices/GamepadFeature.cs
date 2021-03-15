
namespace Raccoon.Input {
    [System.Flags]
    public enum GamepadFeature {
        None                                = 0,

        // thumbsticks axes
        LeftThumbStickHorizontalAxis        = 1 << 0,
        LeftThumbStickVerticalAxis          = 1 << 1,
        RightThumbStickHorizontalAxis       = 1 << 2,
        RightThumbStickVerticalAxis         = 1 << 3,

        // vibration
        LeftVibrationMotor                  = 1 << 4,
        RightVibrationMotor                 = 1 << 5,

        // others
        VoiceSupport                        = 1 << 6,
        LightBar                            = 1 << 7,
        TriggerVibrationMotors              = 1 << 8,
        Misc1                               = 1 << 9,
        Paddle1                             = 1 << 10,
        Paddle2                             = 1 << 11,
        Paddle3                             = 1 << 12,
        Paddle4                             = 1 << 13,
        TouchPad                            = 1 << 14,
        Gyro                                = 1 << 15,
        Accelerometer                       = 1 << 16
    }
}
