
namespace Raccoon.Components {
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ComponentUsageAttribute : System.Attribute {
        public ComponentUsageAttribute() {
            TargetTypes = new System.Type[0];
        }

        public ComponentUsageAttribute(System.Type targetType) {
            if (targetType == null) {
                throw new System.ArgumentNullException(nameof(targetType));
            }

            TargetTypes = new System.Type[] { targetType };
        }

        public ComponentUsageAttribute(System.Type[] targetTypes) {
            if (targetTypes == null) {
                throw new System.ArgumentNullException(nameof(targetTypes));
           }

            for (int i = 0; i < targetTypes.Length; i++) {
                if (targetTypes[i] == null) {
                    throw new System.ArgumentNullException(nameof(targetTypes), $"At target type index #{i}.");
                }
            }

            TargetTypes = targetTypes;
        }

        /// <summary>
        /// Allowed Entities to register <see cref="Component"/>.
        /// Empty array means any <see cref="Entity"/> can register it.
        /// </summary>
        public System.Type[] TargetTypes { get; private set; }

        /// <summary>
        /// Should derivated types of any target type be allowed.
        /// </summary>
        public bool AllowsDerivatedTypes { get; set; } = true;

        /// <summary>
        /// Should <see cref="Component"/> be unique at given <see cref="Entity"/>.
        /// When trying to add another one, <see cref="Entity"/> will raises an exception stating it already has a <see cref="Component"/> with that type.
        /// </summary>
        public bool Unique { get; set; }

        public bool IsEntityTypeAllowed(System.Type entityType) {
            if (TargetTypes.Length == 0) {
                return true;
            }

            if (AllowsDerivatedTypes) {
                foreach (System.Type t in TargetTypes) {
                    if (t.IsAssignableFrom(entityType)) {
                        return true;
                    }
                }
            } else {
                foreach (System.Type t in TargetTypes) {
                    if (t.Equals(entityType)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
