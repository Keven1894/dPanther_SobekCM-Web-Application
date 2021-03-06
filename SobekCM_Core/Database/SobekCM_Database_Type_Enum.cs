﻿namespace SobekCM.Core.Database
{
    /// <summary> Enumeration tells the type of underlying database connection to create </summary>
    public enum SobekCM_Database_Type_Enum : byte
    {
        /// <summary> Microsoft SQL Server </summary>
        MSSQL = 1,

        /// <summary> PostgreSQL Server </summary>
        PostgreSQL
    }
}
