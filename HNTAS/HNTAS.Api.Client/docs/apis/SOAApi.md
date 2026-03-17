# HNTAS.Api.Client.Api.SOAApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiSOAConnectionsPatch**](SOAApi.md#apisoaconnectionspatch) | **PATCH** /api/SOA/connections |  |
| [**ApiSOACreatePost**](SOAApi.md#apisoacreatepost) | **POST** /api/SOA/create |  |
| [**ApiSOADocumentUpdatePatch**](SOAApi.md#apisoadocumentupdatepatch) | **PATCH** /api/SOA/document-update |  |
| [**ApiSOAElementDocumentsPatch**](SOAApi.md#apisoaelementdocumentspatch) | **PATCH** /api/SOA/element-documents |  |
| [**ApiSOAElementLocationsPatch**](SOAApi.md#apisoaelementlocationspatch) | **PATCH** /api/SOA/element-locations |  |
| [**ApiSOAHeatNetworkHnIdGet**](SOAApi.md#apisoaheatnetworkhnidget) | **GET** /api/SOA/heat-network/{hnId} |  |
| [**ApiSOAHnIdDelete**](SOAApi.md#apisoahniddelete) | **DELETE** /api/SOA/{hnId} |  |
| [**ApiSOANetworkElementsPatch**](SOAApi.md#apisoanetworkelementspatch) | **PATCH** /api/SOA/network-elements |  |
| [**ApiSOANetworkTypePatch**](SOAApi.md#apisoanetworktypepatch) | **PATCH** /api/SOA/network-type |  |
| [**ApiSOASendAssessorAssessmentEmailPost**](SOAApi.md#apisoasendassessorassessmentemailpost) | **POST** /api/SOA/send-assessor-assessment-email |  |
| [**ApiSOASendCertificationCompleteEmailPost**](SOAApi.md#apisoasendcertificationcompleteemailpost) | **POST** /api/SOA/send-certification-complete-email |  |
| [**ApiSOAUpdateSoaStatusPatch**](SOAApi.md#apisoaupdatesoastatuspatch) | **PATCH** /api/SOA/update-soa-status |  |
| [**ApiSOAUpdateSoaStatusPut**](SOAApi.md#apisoaupdatesoastatusput) | **PUT** /api/SOA/update-soa-status |  |

<a id="apisoaconnectionspatch"></a>
# **ApiSOAConnectionsPatch**
> void ApiSOAConnectionsPatch (UpdateConnectionsRequest updateConnectionsRequest)




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

<a id="apisoacreatepost"></a>
# **ApiSOACreatePost**
> Soa2 ApiSOACreatePost (string hnId = null, string createdBy = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  | [optional]  |
| **createdBy** | **string** |  | [optional]  |

### Return type

[**Soa2**](Soa2.md)

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

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoadocumentupdatepatch"></a>
# **ApiSOADocumentUpdatePatch**
> void ApiSOADocumentUpdatePatch (UpdateDocumentRequest updateDocumentRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **updateDocumentRequest** | [**UpdateDocumentRequest**](UpdateDocumentRequest.md) |  |  |

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

<a id="apisoaelementdocumentspatch"></a>
# **ApiSOAElementDocumentsPatch**
> void ApiSOAElementDocumentsPatch (UpdateElementDocumentsRequest updateElementDocumentsRequest)




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

<a id="apisoaelementlocationspatch"></a>
# **ApiSOAElementLocationsPatch**
> void ApiSOAElementLocationsPatch (UpdateElementLocationsRequest updateElementLocationsRequest)




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

<a id="apisoaheatnetworkhnidget"></a>
# **ApiSOAHeatNetworkHnIdGet**
> Soa2 ApiSOAHeatNetworkHnIdGet (string hnId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnId** | **string** |  |  |

### Return type

[**Soa2**](Soa2.md)

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

<a id="apisoahniddelete"></a>
# **ApiSOAHnIdDelete**
> void ApiSOAHnIdDelete (string hnId)




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

<a id="apisoanetworkelementspatch"></a>
# **ApiSOANetworkElementsPatch**
> void ApiSOANetworkElementsPatch (List<HeatNetworkElement> heatNetworkElement, string hnId = null, string updatedBy = null)




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

<a id="apisoanetworktypepatch"></a>
# **ApiSOANetworkTypePatch**
> void ApiSOANetworkTypePatch (NetworkTypeSelection2 networkTypeSelection2, string hnId = null, string updatedBy = null)




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

<a id="apisoasendassessorassessmentemailpost"></a>
# **ApiSOASendAssessorAssessmentEmailPost**
> void ApiSOASendAssessorAssessmentEmailPost (string hnName, string hnId, string assessmentResult)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnName** | **string** |  |  |
| **hnId** | **string** |  |  |
| **assessmentResult** | **string** |  |  |

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
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoasendcertificationcompleteemailpost"></a>
# **ApiSOASendCertificationCompleteEmailPost**
> void ApiSOASendCertificationCompleteEmailPost (string hnName, string hnId)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **hnName** | **string** |  |  |
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
| **500** | Internal Server Error |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

<a id="apisoaupdatesoastatuspatch"></a>
# **ApiSOAUpdateSoaStatusPatch**
> void ApiSOAUpdateSoaStatusPatch (ElementSoaStatusUpdateRequest elementSoaStatusUpdateRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **elementSoaStatusUpdateRequest** | [**ElementSoaStatusUpdateRequest**](ElementSoaStatusUpdateRequest.md) |  |  |

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

<a id="apisoaupdatesoastatusput"></a>
# **ApiSOAUpdateSoaStatusPut**
> void ApiSOAUpdateSoaStatusPut (UpdateSoaStatusRequest updateSoaStatusRequest)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **updateSoaStatusRequest** | [**UpdateSoaStatusRequest**](UpdateSoaStatusRequest.md) |  |  |

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
| **204** | No Content |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

