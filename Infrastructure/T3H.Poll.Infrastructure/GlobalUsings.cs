﻿global using T3H.Poll.CrossCuttingConcerns.DateTimes;
global using Microsoft.Extensions.DependencyInjection;
global using Serilog.Events;
global using Serilog.Core;
global using System.Diagnostics;
global using Serilog;
global using System.Reflection;
global using System.Collections.Generic;
global using System;
global using System.IO;
global using System.Linq;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.EventLog;
global using OpenTelemetry.Logs;
global using OpenTelemetry.Resources;
global using OpenTelemetry.Trace;
global using Serilog.Exceptions;
global using System.Net;
global using System.Text.Json;
global using System.Threading;
global using System.Threading.Tasks;
global using T3H.Poll.CrossCuttingConcerns.Exceptions;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Options;
global using Microsoft.AspNetCore.Mvc.ApiExplorer;
global using Microsoft.OpenApi.Models;
global using Swashbuckle.AspNetCore.SwaggerGen;
global using Castle.DynamicProxy;
global using System.Text.Json.Serialization;
global using ILogger = Microsoft.Extensions.Logging.ILogger;
global using T3H.Poll.CrossCuttingConcerns;
global using T3H.Poll.Domain.Repositories;
global using T3H.Poll.Domain.Entities;
global using T3H.Poll.Domain.Identity;
global using Microsoft.AspNetCore.Identity;
namespace T3H.Poll.Infrastructure.Identity;
