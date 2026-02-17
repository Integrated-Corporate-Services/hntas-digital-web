# HNTAS.Api.Client.Api.AuditApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiAuditHeatNetworkHnIdGet**](AuditApi.md#apiauditheatnetworkhnidget) | **GET** /api/Audit/heat-network/{hnId} |  |

<a id="apiauditheatnetworkhnidget"></a>
# **ApiAuditHeatNetworkHnIdGet**
> List&lt;AuditLogResponse&gt; ApiAuditHeatNetworkHnIdGet (string hnId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  |  |

### Return type

[**List&lt;AuditLogResponse&gt;**](AuditLogResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

