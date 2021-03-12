
namespace Raccoon.Input {
    public static class XboxInputLabel {
        [System.Flags]
        public enum Buttons {
            None        = 0,
            A           = 1 << 0, 
            B           = 1 << 1, 
            X           = 1 << 2, 
            Y           = 1 << 3,
            LB          = 1 << 4, 
            RB          = 1 << 5,
            LeftStick   = 1 << 6, 
            RightStick  = 1 << 7,
            DUp         = 1 << 8, 
            DRight      = 1 << 9, 
            DDown       = 1 << 10, 
            DLeft       = 1 << 11,
            Back        = 1 << 12, 
            Start       = 1 << 13,
            BigButton   = 1 << 14
        }

        public enum Triggers {
            LT = 0, 
            RT
        }

        public enum ThumbSticks {
            LeftStick = 0, 
            RightStick
        }

        [System.Flags]
        public enum DPad {
            None    = 0,
            DUp     = 1 << 0, 
            DRight  = 1 << 1, 
            DDown   = 1 << 2, 
            DLeft   = 1 << 3
        }
    }

    public static class XboxInputLabelExtensions {
        public static XboxInputLabel.Buttons ToButton(this XboxInputLabel.DPad dPadLabel) {
            return (XboxInputLabel.Buttons) ((int) dPadLabel << 8);
        }

        public static XboxInputLabel.Buttons ToButton(this XboxInputLabel.ThumbSticks thumbStickLabel) {
            return (XboxInputLabel.Buttons) ((int) thumbStickLabel << 6);
        }
    }
}
