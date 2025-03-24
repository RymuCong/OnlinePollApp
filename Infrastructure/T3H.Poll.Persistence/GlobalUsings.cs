global using FDS.CRM.CrossCuttingConcerns.CircuitBreaker;
global using System;
global using System.Collections.Generic;
global using FDS.CRM.Domain.Repositories;
global using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using System.Data;
global using FDS.CRM.CrossCuttingConcerns.DateTimes;
global using FDS.CRM.CrossCuttingConcerns.Locks;
global using Microsoft.Data.SqlClient;
global using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
global using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;      
global using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
global using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
global using FDS.CRM.Domain.Entities;
global using System.Linq.Expressions;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Logging;
global using System.Reflection;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using System.Data.Common;
global using FDS.CRM.Persistence.Interceptors;
global using FDS.CRM.Persistence.Locks;
global using FDS.CRM.Persistence.Repositories;
global using Microsoft.Extensions.DependencyInjection;
global using FDS.CRM.Domain.Enums;


