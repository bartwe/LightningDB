using Xunit;

namespace LightningDB.Tests {
    [CollectionDefinition("SharedFileSystem")]
    public class SharedFileSystemCollection : ICollectionFixture<SharedFileSystem> { }
}
