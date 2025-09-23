# HNTAS.Api.Client.Api.HeatNetworksApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiHeatNetworksAddHeatNetworkPost**](HeatNetworksApi.md#apiheatnetworksaddheatnetworkpost) | **POST** /api/HeatNetworks/add-heat-network |  |
| [**ApiHeatNetworksGet**](HeatNetworksApi.md#apiheatnetworksget) | **GET** /api/HeatNetworks |  |
| [**ApiHeatNetworksHnIdGet**](HeatNetworksApi.md#apiheatnetworkshnidget) | **GET** /api/HeatNetworks/{hnId} |  |
| [**ApiHeatNetworksHnIdsGet**](HeatNetworksApi.md#apiheatnetworkshnidsget) | **GET** /api/HeatNetworks/hnIds |  |
| [**ApiHeatNetworksHnidAssessorImpartialityGet**](HeatNetworksApi.md#apiheatnetworkshnidassessorimpartialityget) | **GET** /api/HeatNetworks/{hnid}/assessor-impartiality |  |
| [**ApiHeatNetworksHnidAssessorImpartialityPost**](HeatNetworksApi.md#apiheatnetworkshnidassessorimpartialitypost) | **POST** /api/HeatNetworks/{hnid}/assessor-impartiality |  |

<a id="apiheatnetworksaddheatnetworkpost"></a>
# **ApiHeatNetworksAddHeatNetworkPost**
> HeatNetworkResponse ApiHeatNetworksAddHeatNetworkPost (HeatNetwork heatNetwork)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **heatNetwork** | [**HeatNetwork**](HeatNetwork.md) |  |  |

### Return type

[**HeatNetworkResponse**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Created |  -  |
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **409** | Conflict |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworksget"></a>
# **ApiHeatNetworksGet**
> List&lt;HeatNetworkResponse&gt; ApiHeatNetworksGet ()




### Parameters
This endpoint does not need any parameter.
### Return type

[**List&lt;HeatNetworkResponse&gt;**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworkshnidget"></a>
# **ApiHeatNetworksHnIdGet**
> HeatNetworkResponse ApiHeatNetworksHnIdGet (string hnId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  |  |

### Return type

[**HeatNetworkResponse**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworkshnidsget"></a>
# **ApiHeatNetworksHnIdsGet**
> List&lt;HeatNetworkResponse&gt; ApiHeatNetworksHnIdsGet (string hnIdsString = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnIdsString** | **string** |  | [optional]  |

### Return type

[**List&lt;HeatNetworkResponse&gt;**](HeatNetworkResponse.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apiheatnetworkshnidassessorimpartialityget"></a>
# **ApiHeatNetworksHnidAssessorImpartialityGet**
> bool ApiHeatNetworksHnidAssessorImpartialityGet (string hnid)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnid** | **string** |  |  |

### Return type

**bool**

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

<a id="apiheatnetworkshnidassessorimpartialitypost"></a>
# **ApiHeatNetworksHnidAssessorImpartialityPost**
> bool ApiHeatNetworksHnidAssessorImpartialityPost (string hnid)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnid** | **string** |  |  |

### Return type

**bool**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

