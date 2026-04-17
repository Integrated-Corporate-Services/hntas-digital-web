# HNTAS.Api.Client.Api.ArmsApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**V1HnKpiConfigPost**](ArmsApi.md#v1hnkpiconfigpost) | **POST** /v1/hn/kpi-config |  |
| [**V1HnKpisPost**](ArmsApi.md#v1hnkpispost) | **POST** /v1/hn/kpis |  |
| [**V1HnNetworkIdKpiConfigGet**](ArmsApi.md#v1hnnetworkidkpiconfigget) | **GET** /v1/hn/{networkId}/kpi-config |  |

<a id="v1hnkpiconfigpost"></a>
# **V1HnKpiConfigPost**
> void V1HnKpiConfigPost (KpiConfigRequest kpiConfigRequest)




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

<a id="v1hnkpispost"></a>
# **V1HnKpisPost**
> void V1HnKpisPost (KpiSubmissionRequest kpiSubmissionRequest)




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

<a id="v1hnnetworkidkpiconfigget"></a>
# **V1HnNetworkIdKpiConfigGet**
> KpiConfigResponse V1HnNetworkIdKpiConfigGet (string networkId)




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

