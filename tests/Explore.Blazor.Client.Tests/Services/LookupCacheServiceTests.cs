// ABOUTME: Unit tests for LookupCacheService cache behavior and synchronization.
// ABOUTME: Verifies cache hit/miss, invalidation, concurrent access, and safe teardown.

namespace Explore.Blazor.Client.Tests.Services;

public sealed class LookupCacheServiceTests : IDisposable
{
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;
    private readonly IMadhabService _madhabService;
    private readonly ILocationService _locationService;
    private readonly ILanguageService _languageService;
    private readonly ILogger<LookupCacheService> _logger;
    private readonly LookupCacheService _service;

    public LookupCacheServiceTests()
    {
        _categoryService = Substitute.For<ICategoryService>();
        _tagService = Substitute.For<ITagService>();
        _madhabService = Substitute.For<IMadhabService>();
        _locationService = Substitute.For<ILocationService>();
        _languageService = Substitute.For<ILanguageService>();
        _logger = Substitute.For<ILogger<LookupCacheService>>();

        _service = new LookupCacheService(
            _categoryService,
            _tagService,
            _madhabService,
            _locationService,
            _languageService,
            _logger);
    }

    public void Dispose()
    {
        _service.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetCategoriesAsync Tests

    [Test]
    public async Task GetCategoriesAsync_ReturnsFetchedData_WhenCacheIsEmpty()
    {
        // Arrange
        var categories = new List<CategoryListDto> { new() { Id = Guid.NewGuid(), FullName = "Category A", MasterCode = "CAT-A" } };
        _categoryService.GetCategoriesAsync().Returns(categories);

        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(1);
        await Assert.That(result.First().FullName).IsEqualTo("Category A");
    }

    [Test]
    public async Task GetCategoriesAsync_ReturnsCachedData_WhenCacheIsValid()
    {
        // Arrange
        var firstPayload = new List<CategoryListDto> { new() { Id = Guid.NewGuid(), FullName = "Category Original", MasterCode = "CAT-ORIG" } };
        var secondPayload = new List<CategoryListDto> { new() { Id = Guid.NewGuid(), FullName = "Category Changed", MasterCode = "CAT-NEW" } };

        _categoryService.GetCategoriesAsync().Returns(firstPayload);
        _ = await _service.GetCategoriesAsync();
        _categoryService.GetCategoriesAsync().Returns(secondPayload);

        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        await Assert.That(result.First().FullName).IsEqualTo("Category Original");
    }

    [Test]
    public async Task GetCategoriesAsync_ReturnsFreshData_WhenCacheWasInvalidated()
    {
        // Arrange
        var firstPayload = new List<CategoryListDto> { new() { Id = Guid.NewGuid(), FullName = "Category Before", MasterCode = "CAT-BEFORE" } };
        var secondPayload = new List<CategoryListDto> { new() { Id = Guid.NewGuid(), FullName = "Category After", MasterCode = "CAT-AFTER" } };

        _categoryService.GetCategoriesAsync().Returns(firstPayload);
        _ = await _service.GetCategoriesAsync();
        _categoryService.GetCategoriesAsync().Returns(secondPayload);
        _service.InvalidateAll();

        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        await Assert.That(result.First().FullName).IsEqualTo("Category After");
    }

    [Test]
    public async Task GetCategoriesAsync_PropagatesApiException_WhenUnderlyingServiceFails()
    {
        // Arrange
        _categoryService.GetCategoriesAsync().ThrowsAsync(new ApiException("Error", 500, null, null, null));

        // Act & Assert
        await Assert.That(() => _service.GetCategoriesAsync()).ThrowsExactly<ApiException>();
    }

    [Test]
    public async Task GetCategoriesAsync_FetchesOnce_WhenCalledConcurrently()
    {
        // Arrange
        var categories = new List<CategoryListDto> { new() { Id = Guid.NewGuid(), FullName = "Concurrent", MasterCode = "CAT-C" } };
        var fetchCount = 0;

        _categoryService.GetCategoriesAsync().Returns(_ =>
        {
            Interlocked.Increment(ref fetchCount);
            return Task.FromResult<ICollection<CategoryListDto>>(categories);
        });

        // Act
        var task1 = _service.GetCategoriesAsync();
        var task2 = _service.GetCategoriesAsync();
        await Task.WhenAll(task1, task2);

        // Assert
        await Assert.That(fetchCount).IsEqualTo(1);
        await Assert.That(task1.Result.First().FullName).IsEqualTo("Concurrent");
        await Assert.That(task2.Result.First().FullName).IsEqualTo("Concurrent");
    }

    [Test]
    public async Task GetCategoriesAsync_WhenDisposedDuringInFlightFetch_CompletesWithoutDisposedSemaphoreError()
    {
        // Arrange
        var fetchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var fetchResult = new TaskCompletionSource<ICollection<CategoryListDto>>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _categoryService.GetCategoriesAsync().Returns(_ =>
        {
            fetchStarted.SetResult();
            return fetchResult.Task;
        });

        var loadTask = _service.GetCategoriesAsync();

        // Act
        await fetchStarted.Task;
        _service.Dispose();
        fetchResult.SetResult([
            new CategoryListDto
            {
                Id = Guid.NewGuid(),
                FullName = "Safe Teardown",
                MasterCode = "SAFE"
            }
        ]);

        var result = await loadTask;

        // Assert
        await Assert.That(result.First().FullName).IsEqualTo("Safe Teardown");
    }

    #endregion

    #region GetTagsAsync Tests

    [Test]
    public async Task GetTagsAsync_ReturnsFetchedData_WhenCacheIsEmpty()
    {
        // Arrange
        var tags = new List<TagListDto> { new() { Id = Guid.NewGuid(), FullName = "Tag A", MasterCode = "TAG-A" } };
        _tagService.GetTagsAsync().Returns(tags);

        // Act
        var result = await _service.GetTagsAsync();

        // Assert
        await Assert.That(result.Count).IsEqualTo(1);
        await Assert.That(result.First().FullName).IsEqualTo("Tag A");
    }

    [Test]
    public async Task GetTagsAsync_ReturnsCachedData_WhenCacheIsValid()
    {
        // Arrange
        var firstPayload = new List<TagListDto> { new() { Id = Guid.NewGuid(), FullName = "Tag Original", MasterCode = "TAG-ORIG" } };
        var secondPayload = new List<TagListDto> { new() { Id = Guid.NewGuid(), FullName = "Tag Changed", MasterCode = "TAG-NEW" } };

        _tagService.GetTagsAsync().Returns(firstPayload);
        _ = await _service.GetTagsAsync();
        _tagService.GetTagsAsync().Returns(secondPayload);

        // Act
        var result = await _service.GetTagsAsync();

        // Assert
        await Assert.That(result.First().FullName).IsEqualTo("Tag Original");
    }

    [Test]
    public async Task GetTagsAsync_PropagatesApiException_WhenUnderlyingServiceFails()
    {
        // Arrange
        _tagService.GetTagsAsync().ThrowsAsync(new ApiException("Error", 500, null, null, null));

        // Act & Assert
        await Assert.That(() => _service.GetTagsAsync()).ThrowsExactly<ApiException>();
    }

    #endregion

    #region InvalidateAll Tests

    [Test]
    public async Task InvalidateAll_RefreshesRepresentativeCaches_WhenCalled()
    {
        // Arrange
        var categoryBefore = new List<CategoryListDto> { new() { Id = Guid.NewGuid(), FullName = "Category Before", MasterCode = "CAT-B" } };
        var categoryAfter = new List<CategoryListDto> { new() { Id = Guid.NewGuid(), FullName = "Category After", MasterCode = "CAT-A" } };
        var tagBefore = new List<TagListDto> { new() { Id = Guid.NewGuid(), FullName = "Tag Before", MasterCode = "TAG-B" } };
        var tagAfter = new List<TagListDto> { new() { Id = Guid.NewGuid(), FullName = "Tag After", MasterCode = "TAG-A" } };

        _categoryService.GetCategoriesAsync().Returns(categoryBefore);
        _tagService.GetTagsAsync().Returns(tagBefore);
        _ = await _service.GetCategoriesAsync();
        _ = await _service.GetTagsAsync();

        _categoryService.GetCategoriesAsync().Returns(categoryAfter);
        _tagService.GetTagsAsync().Returns(tagAfter);
        _service.InvalidateAll();

        // Act
        var categories = await _service.GetCategoriesAsync();
        var tags = await _service.GetTagsAsync();

        // Assert
        await Assert.That(categories.First().FullName).IsEqualTo("Category After");
        await Assert.That(tags.First().FullName).IsEqualTo("Tag After");
    }

    #endregion
}
