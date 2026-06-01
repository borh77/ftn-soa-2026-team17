using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace GatewayApi.Swagger;

public class TranscodedToursDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // /tours (GetByAuthor)
        var toursPath = new OpenApiPathItem();

        var getByAuthorOp = new OpenApiOperation
        {
            Summary = "Get tours by author (transcoded to gRPC)",
            Description = "Gateway REST -> gRPC GetByAuthor",
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "OK",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema { Type = "object" }
                        }
                    }
                }
            },
            Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter { Name = "authorId", In = ParameterLocation.Query, Required = true, Schema = new OpenApiSchema { Type = "integer", Format = "int64" } },
                new OpenApiParameter { Name = "page", In = ParameterLocation.Query, Required = false, Schema = new OpenApiSchema { Type = "integer", Format = "int32", Default = new OpenApiInteger(1) } },
                new OpenApiParameter { Name = "pageSize", In = ParameterLocation.Query, Required = false, Schema = new OpenApiSchema { Type = "integer", Format = "int32", Default = new OpenApiInteger(10) } }
            }
        };

        toursPath.AddOperation(OperationType.Get, getByAuthorOp);

        // /tours/active (GetActive)
        var activePath = new OpenApiPathItem();

        var getActiveOp = new OpenApiOperation
        {
            Summary = "Get active tours (transcoded to gRPC)",
            Description = "Gateway REST -> gRPC GetActive",
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "OK",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema { Type = "object" }
                        }
                    }
                }
            },
            Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter { Name = "page", In = ParameterLocation.Query, Required = false, Schema = new OpenApiSchema { Type = "integer", Format = "int32", Default = new OpenApiInteger(1) } },
                new OpenApiParameter { Name = "pageSize", In = ParameterLocation.Query, Required = false, Schema = new OpenApiSchema { Type = "integer", Format = "int32", Default = new OpenApiInteger(10) } }
            }
        };

        activePath.AddOperation(OperationType.Get, getActiveOp);

        var startTourPath = new OpenApiPathItem();
        startTourPath.AddOperation(OperationType.Post, new OpenApiOperation
        {
            Summary = "Start tour execution (transcoded to gRPC)",
            Description = "Gateway REST -> gRPC StartTour",
            Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter { Name = "tourId", In = ParameterLocation.Path, Required = true, Schema = new OpenApiSchema { Type = "integer", Format = "int64" } }
            },
            RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties =
                            {
                                ["latitude"] = new OpenApiSchema { Type = "number", Format = "double" },
                                ["longitude"] = new OpenApiSchema { Type = "number", Format = "double" }
                            }
                        }
                    }
                }
            },
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Description = "OK" }
            }
        });

        var checkKeyPointsPath = new OpenApiPathItem();
        checkKeyPointsPath.AddOperation(OperationType.Post, new OpenApiOperation
        {
            Summary = "Check key point proximity (transcoded to gRPC)",
            Description = "Gateway REST -> gRPC CheckKeyPointProximity",
            Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter { Name = "executionId", In = ParameterLocation.Path, Required = true, Schema = new OpenApiSchema { Type = "integer", Format = "int64" } }
            },
            RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties =
                            {
                                ["latitude"] = new OpenApiSchema { Type = "number", Format = "double" },
                                ["longitude"] = new OpenApiSchema { Type = "number", Format = "double" }
                            }
                        }
                    }
                }
            },
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Description = "OK" }
            }
        });

        // Inject into document if not present
        if (!swaggerDoc.Paths.ContainsKey("/tours"))
            swaggerDoc.Paths.Add("/tours", toursPath);

        if (!swaggerDoc.Paths.ContainsKey("/tours/active"))
            swaggerDoc.Paths.Add("/tours/active", activePath);

        if (!swaggerDoc.Paths.ContainsKey("/tour-executions/tours/{tourId}"))
            swaggerDoc.Paths.Add("/tour-executions/tours/{tourId}", startTourPath);

        if (!swaggerDoc.Paths.ContainsKey("/tour-executions/{executionId}/check-keypoints"))
            swaggerDoc.Paths.Add("/tour-executions/{executionId}/check-keypoints", checkKeyPointsPath);
    }
}
