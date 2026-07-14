# HNTAS.Api.Client.Api.ImportApi

All URIs are relative to *https://localhost:7117*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiImportUploadCsvPost**](ImportApi.md#apiimportuploadcsvpost) | **POST** /api/Import/upload-csv |  |

<a id="apiimportuploadcsvpost"></a>
# **ApiImportUploadCsvPost**
> ImportResult ApiImportUploadCsvPost (string fileContent = null)




### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fileContent** | **string** |  | [optional]  |

### Return type

[**ImportResult**](ImportResult.md)

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
| **499** | Client Closed Request |  -  |

[[Back to top]](#) [[Back to API list]](../../README.md#documentation-for-api-endpoints) [[Back to Model list]](../../README.md#documentation-for-models) [[Back to README]](../../README.md)

