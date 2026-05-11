namespace OVOSUR.Api.Modules.Security.Entities;

public static class UserTypes
{
    public const string Interno = "INTERNO";
    public const string Proveedor = "PROVEEDOR";
}

public static class ApprovalStates
{
    public const string Pendiente = "PENDIENTE";
    public const string Aprobado = "APROBADO";
    public const string Rechazado = "RECHAZADO";
}

public static class RoleCodes
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string Proveedor = "PROVEEDOR";
}
