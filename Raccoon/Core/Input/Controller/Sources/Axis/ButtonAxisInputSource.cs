using System.Collections.Generic;
using System.Text;

namespace Raccoon.Input {
    public class ButtonAxisInputSource<D> : AxisInputSource<D> where D : InputDevice {
        #region Private Members

        private List<ButtonInputSource<D>> _upButtonSources,
                                           _rightButtonSources,
                                           _downButtonSources,
                                           _leftButtonSources;

        #endregion Private Members

        #region Constructors

        public ButtonAxisInputSource(D device) : base(device) {
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Should inputs sum up instead being absolute set.
        /// </summary>
        /// <remarks>
        /// When Left and Right inputs together results into X being zero. (the same applies to Up and Down)
        /// Not being additive results into an absolute input behavior.
        /// So when opposite sides inputs together, one will always overrides the other. 
        /// </remarks>
        public bool AdditiveInput { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void Update(int delta) {
            base.Update(delta);
            float x, y;

            UpdateSources(ref delta, ref _upButtonSources);
            UpdateSources(ref delta, ref _rightButtonSources);
            UpdateSources(ref delta, ref _downButtonSources);
            UpdateSources(ref delta, ref _leftButtonSources);

            if (AdditiveInput) {
                x = y = 0f;

                // horizontal
                if (IsAnyButtonDown(ref _leftButtonSources)) {
                    x -= 1f;
                }

                if (IsAnyButtonDown(ref _rightButtonSources)) {
                    x += 1f;
                }

                // vertical
                if (IsAnyButtonDown(ref _upButtonSources)) {
                    y -= 1f;
                }

                if (IsAnyButtonDown(ref _downButtonSources)) {
                    y += 1f;
                }
            } else {
                x = X;
                y = Y;

                // horizontal
                if (x < 0f) {
                    // left button is already pressed

                    if (!IsAnyButtonDown(ref _leftButtonSources)) {
                        if (IsAnyButtonDown(ref _rightButtonSources)) {
                            x = 1f;
                        } else {
                            x = 0f;
                        }
                    }
                } else if (x > 0f) {
                    // right button is already pressed

                    if (!IsAnyButtonDown(ref _rightButtonSources)) {
                        if (IsAnyButtonDown(ref _leftButtonSources)) {
                            x = -1f;
                        } else {
                            x = 0f;
                        }
                    }
                } else {
                    if (IsAnyButtonDown(ref _leftButtonSources)) {
                        x = -1f;
                    } else if (IsAnyButtonDown(ref _rightButtonSources)) {
                        x = 1f;
                    } else {
                        x = 0f;
                    }
                }

                // vertical
                if (y < 0f) {
                    // up button is already pressed

                    if (!IsAnyButtonDown(ref _upButtonSources)) {
                        if (IsAnyButtonDown(ref _downButtonSources)) {
                            y = 1f;
                        } else {
                            y = 0f;
                        }
                    }
                } else if (y > 0f) {
                    // down button is already pressed

                    if (!IsAnyButtonDown(ref _downButtonSources)) {
                        if (IsAnyButtonDown(ref _upButtonSources)) {
                            y = -1f;
                        } else {
                            y = 0f;
                        }
                    }
                } else {
                    if (IsAnyButtonDown(ref _upButtonSources)) {
                        y = -1f;
                    } else if (IsAnyButtonDown(ref _downButtonSources)) {
                        y = 1f;
                    } else {
                        y = 0f;
                    }
                }
            }

            X = x;
            Y = y;
        }

        public void AddUp(ButtonInputSource<D> source) {
            if (source == null) {
                throw new System.ArgumentNullException(nameof(source));
            }

            if (_upButtonSources == null) {
                _upButtonSources = new List<ButtonInputSource<D>>();
            }

            _upButtonSources.Add(source);
        }

        public void AddRight(ButtonInputSource<D> source) {
            if (source == null) {
                throw new System.ArgumentNullException(nameof(source));
            }

            if (_rightButtonSources == null) {
                _rightButtonSources = new List<ButtonInputSource<D>>();
            }

            _rightButtonSources.Add(source);
        }

        public void AddDown(ButtonInputSource<D> source) {
            if (source == null) {
                throw new System.ArgumentNullException(nameof(source));
            }

            if (_downButtonSources == null) {
                _downButtonSources = new List<ButtonInputSource<D>>();
            }

            _downButtonSources.Add(source);
        }

        public void AddLeft(ButtonInputSource<D> source) {
            if (source == null) {
                throw new System.ArgumentNullException(nameof(source));
            }

            if (_leftButtonSources == null) {
                _leftButtonSources = new List<ButtonInputSource<D>>();
            }

            _leftButtonSources.Add(source);
        }

        public IEnumerable<ButtonInputSource<D>> UpSources() {
            foreach (ButtonInputSource<D> source in _upButtonSources) {
                yield return source;
            }
        }

        public IEnumerable<ButtonInputSource<D>> RightSources() {
            foreach (ButtonInputSource<D> source in _rightButtonSources) {
                yield return source;
            }
        }

        public IEnumerable<ButtonInputSource<D>> DownSources() {
            foreach (ButtonInputSource<D> source in _downButtonSources) {
                yield return source;
            }
        }

        public IEnumerable<ButtonInputSource<D>> LeftSources() {
            foreach (ButtonInputSource<D> source in _leftButtonSources) {
                yield return source;
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Button Axis ");

            // up
            sb.Append("[");
            if (_upButtonSources != null) {
                foreach (ButtonInputSource<D> source in _upButtonSources) {
                    sb.Append(source.ToString());
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2); // remove last separator
            }
            sb.Append("], ");

            // right
            sb.Append("[");
            if (_rightButtonSources != null) {
                foreach (ButtonInputSource<D> source in _rightButtonSources) {
                    sb.Append(source.ToString());
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2); // remove last separator
            }
            sb.Append("], ");

            // down
            sb.Append("[");
            if (_downButtonSources != null) {
                foreach (ButtonInputSource<D> source in _downButtonSources) {
                    sb.Append(source.ToString());
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2); // remove last separator
            }
            sb.Append("], ");

            // left
            sb.Append("[");
            if (_leftButtonSources != null) {
                foreach (ButtonInputSource<D> source in _leftButtonSources) {
                    sb.Append(source.ToString());
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2); // remove last separator
            }
            sb.Append("]");

            return sb.ToString();
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateSources(ref int delta, ref List<ButtonInputSource<D>> buttonSources) {
            if (buttonSources == null) {
                return;
            }

            foreach (ButtonInputSource<D> source in buttonSources) {
                source.Update(delta);
            }
        }

        private bool IsAnyButtonDown(ref List<ButtonInputSource<D>> buttonSources) {
            if (buttonSources == null) {
                return false;
            }

            foreach (ButtonInputSource<D> buttonSource in buttonSources) {
                if (buttonSource.IsDown) {
                    return true;
                }
            }

            return false;
        }

        #endregion Private Methods
    }
}
