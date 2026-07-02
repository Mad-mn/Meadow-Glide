using ByteBrewSDK;

namespace Feature.ByteBrewModule.Scripts {
    public class ByteBrewInitializeService : IByteBrewInitializeService {
        public void Initialize() {
            ByteBrew.InitializeByteBrew();
        }
    }
}