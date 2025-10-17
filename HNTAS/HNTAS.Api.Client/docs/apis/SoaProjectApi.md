# HNTAS.Api.Client.Api.SoaProjectApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiSoaProjectAssessmentPlanPost**](SoaProjectApi.md#apisoaprojectassessmentplanpost) | **POST** /api/SoaProject/assessment-plan |  |
| [**ApiSoaProjectConnectionsPatch**](SoaProjectApi.md#apisoaprojectconnectionspatch) | **PATCH** /api/SoaProject/connections |  |
| [**ApiSoaProjectCreatePost**](SoaProjectApi.md#apisoaprojectcreatepost) | **POST** /api/SoaProject/create |  |
| [**ApiSoaProjectElementDocumentsPost**](SoaProjectApi.md#apisoaprojectelementdocumentspost) | **POST** /api/SoaProject/element-documents |  |
| [**ApiSoaProjectElementLocationsPost**](SoaProjectApi.md#apisoaprojectelementlocationspost) | **POST** /api/SoaProject/element-locations |  |
| [**ApiSoaProjectHeatNetworkHnIdGet**](SoaProjectApi.md#apisoaprojectheatnetworkhnidget) | **GET** /api/SoaProject/heat-network/{hnId} |  |
| [**ApiSoaProjectHnIdDelete**](SoaProjectApi.md#apisoaprojecthniddelete) | **DELETE** /api/SoaProject/{hnId} |  |
| [**ApiSoaProjectNetworkElementsPatch**](SoaProjectApi.md#apisoaprojectnetworkelementspatch) | **PATCH** /api/SoaProject/network-elements |  |
| [**ApiSoaProjectNetworkTypePatch**](SoaProjectApi.md#apisoaprojectnetworktypepatch) | **PATCH** /api/SoaProject/network-type |  |
| [**ApiSoaProjectProjectIdGet**](SoaProjectApi.md#apisoaprojectprojectidget) | **GET** /api/SoaProject/{projectId} |  |

<a id="apisoaprojectassessmentplanpost"></a>
# **ApiSoaProjectAssessmentPlanPost**
> void ApiSoaProjectAssessmentPlanPost (UpdateAssessmentPlanRequest updateAssessmentPlanRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **updateAssessmentPlanRequest** | [**UpdateAssessmentPlanRequest**](UpdateAssessmentPlanRequest.md) |  |  |

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoaprojectconnectionspatch"></a>
# **ApiSoaProjectConnectionsPatch**
> void ApiSoaProjectConnectionsPatch (UpdateConnectionsRequest updateConnectionsRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **updateConnectionsRequest** | [**UpdateConnectionsRequest**](UpdateConnectionsRequest.md) |  |  |

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoaprojectcreatepost"></a>
# **ApiSoaProjectCreatePost**
> SoaProject ApiSoaProjectCreatePost (string hnId = null, string createdBy = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  | [optional]  |
| **createdBy** | **string** |  | [optional]  |

### Return type

[**SoaProject**](SoaProject.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Created |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoaprojectelementdocumentspost"></a>
# **ApiSoaProjectElementDocumentsPost**
> void ApiSoaProjectElementDocumentsPost (UpdateElementDocumentsRequest updateElementDocumentsRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **updateElementDocumentsRequest** | [**UpdateElementDocumentsRequest**](UpdateElementDocumentsRequest.md) |  |  |

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoaprojectelementlocationspost"></a>
# **ApiSoaProjectElementLocationsPost**
> void ApiSoaProjectElementLocationsPost (UpdateElementLocationsRequest updateElementLocationsRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **updateElementLocationsRequest** | [**UpdateElementLocationsRequest**](UpdateElementLocationsRequest.md) |  |  |

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoaprojectheatnetworkhnidget"></a>
# **ApiSoaProjectHeatNetworkHnIdGet**
> SoaProject ApiSoaProjectHeatNetworkHnIdGet (string hnId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  |  |

### Return type

[**SoaProject**](SoaProject.md)

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

<a id="apisoaprojecthniddelete"></a>
# **ApiSoaProjectHnIdDelete**
> void ApiSoaProjectHnIdDelete (string hnId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  |  |

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **204** | No Content |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoaprojectnetworkelementspatch"></a>
# **ApiSoaProjectNetworkElementsPatch**
> void ApiSoaProjectNetworkElementsPatch (List<HeatNetworkElement> heatNetworkElement, string hnId = null, string updatedBy = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **heatNetworkElement** | [**List&lt;HeatNetworkElement&gt;**](HeatNetworkElement.md) |  |  |
| **hnId** | **string** |  | [optional]  |
| **updatedBy** | **string** |  | [optional]  |

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoaprojectnetworktypepatch"></a>
# **ApiSoaProjectNetworkTypePatch**
> void ApiSoaProjectNetworkTypePatch (NetworkTypeSelection2 networkTypeSelection2, string hnId = null, string updatedBy = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **networkTypeSelection2** | [**NetworkTypeSelection2**](NetworkTypeSelection2.md) |  |  |
| **hnId** | **string** |  | [optional]  |
| **updatedBy** | **string** |  | [optional]  |

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoaprojectprojectidget"></a>
# **ApiSoaProjectProjectIdGet**
> SoaProject ApiSoaProjectProjectIdGet (string projectId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **projectId** | **string** |  |  |

### Return type

[**SoaProject**](SoaProject.md)

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

