using System.IO;
using System.Reflection;

using Raccoon.FileSystem;
using Raccoon.Data.Consumers;
using Raccoon.Data.Parsers;

namespace Raccoon.Data {
    public class DataFile {
        private static Parser Parser = new Parser();
        private static Saver Saver = new Saver();

        public DataFile() {
        }

        /// <summary>
        /// Parse a data file and inject values at provided target.
        /// </summary>
#if DEBUG
        public static void Parse<T>(PathBuf filepath, T target, DataTokensConsumer consumer, bool debug = false) {
#else
        public static void Parse<T>(PathBuf filepath, T target, DataTokensConsumer consumer) {
#endif
            if (filepath == null) {
                throw new System.ArgumentNullException(nameof(filepath));
            }

            if (target == null) {
                throw new System.ArgumentNullException(nameof(target));
            }

            if (consumer == null) {
                throw new System.ArgumentNullException(nameof(consumer));
            }

            // parse file
            ListToken<Token> rootToken;
            using (StreamReader stream = new StreamReader(
                filepath.ToString(),
                System.Text.Encoding.UTF8
            )) {
#if DEBUG
                rootToken = Parser.Parse(stream, debug);
#else
                rootToken = Parser.Parse(stream);
#endif
            }

            if (rootToken != null) {
                System.Type type = typeof(T);

                // gather data contract
                DataContract contract = new DataContract(
                    type,
                    type.GetCustomAttribute<DataContractAttribute>()
                );

                // inject data to target
                consumer.Consume(target, contract, rootToken);
            }

            //

            Parser.Reset();
        }

#if DEBUG
        public static void Parse<T>(PathBuf filepath, T target, bool debug = false) {
            Parse<T>(filepath, target, SimpleDataTokensConsumer.Instance, debug);
        }
#else
        public static void Parse<T>(PathBuf filepath, T target) {
            Parse<T>(filepath, target, SimpleDataTokensConsumer.Instance);
        }
#endif

        public static void Save<T>(PathBuf filepath, T target, SaveSettings? settings = null) {
            if (filepath == null) {
                throw new System.ArgumentNullException(nameof(filepath));
            }

            if (target == null) {
                throw new System.ArgumentNullException(nameof(target));
            }

            System.Type type = typeof(T);

            // gather data contract
            DataContract contract = new DataContract(
                type,
                type.GetCustomAttribute<DataContractAttribute>()
            );

            // extract data from target
            ListToken<Token> rootToken
                = SimpleDataTokensProducer.Instance.Produce(target, contract);

            // write file
            using (StreamWriter stream = new StreamWriter(
                filepath.ToString(),
                false,
                System.Text.Encoding.UTF8
            )) {
                Saver.Save(stream, rootToken, settings);
            }

            //

            Saver.Reset();
        }
    }

    [DataContract]
    public class TestData {
        public TestData() {
        }

        [DataField]
        public TestVideoData Video { get; set; } = new TestVideoData();

        [DataField]
        public TestAudioData Audio { get; set; } = new TestAudioData();

        [DataField]
        public TestGameData Game { get; set; } = new TestGameData();

        public override string ToString() {
            return $@"TestData
Video:
    {Video}

Audio:
    {Audio}

Game:
    {Game}";
        }
    }

    [DataContract]
    public class TestVideoData {
        public TestVideoData() {
        }

        [DataField]
        public int Width { get; set; }

        [DataField]
        public int Height { get; set; }

        public override string ToString() {
            return $@"Width: {Width}
Height: {Height}";
        }
    }

    [DataContract]
    public class TestAudioData {
        public TestAudioData() {
        }

        [DataField]
        public int Volume { get; set; }

        [DataField]
        public int MaxVolume { get; set; }

        public override string ToString() {
            return $@"Volume: {Volume}
MaxVolume: {MaxVolume}";
        }
    }

    [DataContract]
    public class TestGameData {
        public TestGameData() {
        }

        [DataField]
        public int PixelScale { get; set; }

        public override string ToString() {
            return $@"PixelScale: {PixelScale}";
        }
    }
}
