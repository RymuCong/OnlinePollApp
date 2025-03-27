global using T3H.Poll.CrossCuttingConcerns.CircuitBreaker;
global using System;
global using System.Collections.Generic;
global using T3H.Poll.Domain.Repositories;
global using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using System.Data;
global using T3H.Poll.CrossCuttingConcerns.DateTimes;
global using T3H.Poll.CrossCuttingConcerns.Locks;
global using Microsoft.Data.SqlClient;
global using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
global using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;      
global using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
global using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
global using T3H.Poll.Domain.Entities;
global using System.Linq.Expressions;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Logging;
global using System.Reflection;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using System.Data.Common;
global using T3H.Poll.Persistence.Interceptors;
global using T3H.Poll.Persistence.Locks;
global using T3H.Poll.Persistence.Repositories;
global using Microsoft.Extensions.DependencyInjection;
global using T3H.Poll.Domain.Enums;


