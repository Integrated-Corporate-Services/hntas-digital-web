# HNTAS.Api.Client.Api.HeatNetworksApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiHeatNetworksAddHeatNetworkPost**](HeatNetworksApi.md#apiheatnetworksaddheatnetworkpost) | **POST** /api/HeatNetworks/add-heat-network |  |
| [**ApiHeatNetworksGet**](HeatNetworksApi.md#apiheatnetworksget) | **GET** /api/HeatNetworks |  |
| [**ApiHeatNetworksHnIdsGet**](HeatNetworksApi.md#apiheatnetworkshnidsget) | **GET** /api/HeatNetworks/hnIds |  |

<a id="apiheatnetworksaddheatnetworkpost"></a>
# **ApiHeatNetworksAddHeatNetworkPost**
> HeatNetwork ApiHeatNetworksAddHeatNetworkPost (HeatNetwork heatNetwork)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **heatNetwork** | [**HeatNetwork**](HeatNetwork.md) |  |  |

### Return type

[**HeatNetwork**](HeatNetwork.md)

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
> List&lt;HeatNetwork&gt; ApiHeatNetworksGet ()




### Parameters
This endpoint does not need any parameter.
### Return type

[**List&lt;HeatNetwork&gt;**](HeatNetwork.md)

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

<a id="apiheatnetworkshnidsget"></a>
# **ApiHeatNetworksHnIdsGet**
> List&lt;HeatNetwork&gt; ApiHeatNetworksHnIdsGet (string hnIdsString = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnIdsString** | **string** |  | [optional]  |

### Return type

[**List&lt;HeatNetwork&gt;**](HeatNetwork.md)

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

