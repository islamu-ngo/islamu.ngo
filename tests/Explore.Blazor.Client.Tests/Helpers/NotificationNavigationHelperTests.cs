// ABOUTME: Verifies notification entity deep links match the Blazouter route table.
// ABOUTME: Covers organization, group, and unsupported notification entity targets.

using Explore.Blazor.Client.Helpers;

namespace Explore.Blazor.Client.Tests.Helpers;

public sealed class NotificationNavigationHelperTests
{
    [Test]
    public async Task GetEntityUrl_WithOrganizationNotification_ReturnsOrganizationProfileRoute()
    {
        var actorId = Guid.NewGuid().ToString();
        var notification = CreateNotification("Organization", actorId);

        var url = NotificationNavigationHelper.GetEntityUrl(notification);

        await Assert.That(url).IsEqualTo($"/organization/profile/{actorId}");
    }

    [Test]
    public async Task GetEntityUrl_WithGroupNotification_ReturnsGroupProfileRoute()
    {
        var actorId = Guid.NewGuid().ToString();
        var notification = CreateNotification("Group", actorId);

        var url = NotificationNavigationHelper.GetEntityUrl(notification);

        await Assert.That(url).IsEqualTo($"/group/profile/{actorId}");
    }

    [Test]
    public async Task GetEntityUrl_WithUnsupportedNotification_ReturnsNull()
    {
        var notification = CreateNotification("Unknown", Guid.NewGuid().ToString());

        var url = NotificationNavigationHelper.GetEntityUrl(notification);

        await Assert.That(url).IsNull();
    }

    private static NotificationListDto CreateNotification(string entityTypeName, string entityId)
    {
        return new NotificationListDto
        {
            Title = "Notification",
            NotificationEntityTypeName = entityTypeName,
            EntityId = entityId
        };
    }
}
