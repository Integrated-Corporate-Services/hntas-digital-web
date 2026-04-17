# HNTAS.Api.Client.Api.ArmsApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiArmsV1HnKpiConfigPost**](ArmsApi.md#apiarmsv1hnkpiconfigpost) | **POST** /api/arms/v1/hn/kpi-config |  |
| [**ApiArmsV1HnKpisPost**](ArmsApi.md#apiarmsv1hnkpispost) | **POST** /api/arms/v1/hn/kpis |  |
| [**ApiArmsV1HnNetworkIdKpiConfigGet**](ArmsApi.md#apiarmsv1hnnetworkidkpiconfigget) | **GET** /api/arms/v1/hn/{networkId}/kpi-config |  |

<a id="apiarmsv1hnkpiconfigpost"></a>
# **ApiArmsV1HnKpiConfigPost**
> void ApiArmsV1HnKpiConfigPost (KpiConfigRequest kpiConfigRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **kpiConfigRequest** | [**KpiConfigRequest**](KpiConfigRequest.md) |  |  |

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiarmsv1hnkpispost"></a>
# **ApiArmsV1HnKpisPost**
> void ApiArmsV1HnKpisPost (KpiSubmissionRequest kpiSubmissionRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **kpiSubmissionRequest** | [**KpiSubmissionRequest**](KpiSubmissionRequest.md) |  |  |

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **500** | Internal Server Error |  -  |
| **503** | Service Unavailable |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiarmsv1hnnetworkidkpiconfigget"></a>
# **ApiArmsV1HnNetworkIdKpiConfigGet**
> KpiConfigResponse ApiArmsV1HnNetworkIdKpiConfigGet (string networkId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **networkId** | **string** |  |  |

### Return type

[**KpiConfigResponse**](KpiConfigResponse.md)

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
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

