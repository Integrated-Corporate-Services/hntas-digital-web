# HNTAS.Api.Client.Api.ArmsApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ArmsV1HnKpiConfigPost**](ArmsApi.md#armsv1hnkpiconfigpost) | **POST** /arms/v1/hn/kpi-config |  |
| [**ArmsV1HnKpisPost**](ArmsApi.md#armsv1hnkpispost) | **POST** /arms/v1/hn/kpis |  |
| [**ArmsV1HnNetworkIdKpiConfigGet**](ArmsApi.md#armsv1hnnetworkidkpiconfigget) | **GET** /arms/v1/hn/{networkId}/kpi-config |  |
| [**ArmsV2HnKpiConfigPost**](ArmsApi.md#armsv2hnkpiconfigpost) | **POST** /arms/v2/hn/kpi-config |  |
| [**ArmsV2HnKpisPost**](ArmsApi.md#armsv2hnkpispost) | **POST** /arms/v2/hn/kpis |  |
| [**ArmsV2HnNetworkIdKpiConfigGet**](ArmsApi.md#armsv2hnnetworkidkpiconfigget) | **GET** /arms/v2/hn/{networkId}/kpi-config |  |

<a id="armsv1hnkpiconfigpost"></a>
# **ArmsV1HnKpiConfigPost**
> void ArmsV1HnKpiConfigPost (KpiConfigRequest kpiConfigRequest)




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

<a id="armsv1hnkpispost"></a>
# **ArmsV1HnKpisPost**
> void ArmsV1HnKpisPost (KpiSubmissionRequest kpiSubmissionRequest)




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

<a id="armsv1hnnetworkidkpiconfigget"></a>
# **ArmsV1HnNetworkIdKpiConfigGet**
> KpiConfigResponse ArmsV1HnNetworkIdKpiConfigGet (string networkId)




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

<a id="armsv2hnkpiconfigpost"></a>
# **ArmsV2HnKpiConfigPost**
> void ArmsV2HnKpiConfigPost (KpiConfigRequestV2 kpiConfigRequestV2)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **kpiConfigRequestV2** | [**KpiConfigRequestV2**](KpiConfigRequestV2.md) |  |  |

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

<a id="armsv2hnkpispost"></a>
# **ArmsV2HnKpisPost**
> void ArmsV2HnKpisPost (KpiSubmissionRequestV2 kpiSubmissionRequestV2)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **kpiSubmissionRequestV2** | [**KpiSubmissionRequestV2**](KpiSubmissionRequestV2.md) |  |  |

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

<a id="armsv2hnnetworkidkpiconfigget"></a>
# **ArmsV2HnNetworkIdKpiConfigGet**
> KpiConfigResponseV2 ArmsV2HnNetworkIdKpiConfigGet (string networkId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **networkId** | **string** |  |  |

### Return type

[**KpiConfigResponseV2**](KpiConfigResponseV2.md)

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

