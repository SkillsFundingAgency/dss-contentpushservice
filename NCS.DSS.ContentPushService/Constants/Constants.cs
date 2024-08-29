namespace NCS.DSS.ContentPushService.Constants
{
    [Serializable()]
    public enum DigitalIdentityServiceActions
    {
        SuccessfullyActioned = 1,
        DeadLettered = 2,
        Requeued = 3,
        CouldNotAction = 4
    }
}
