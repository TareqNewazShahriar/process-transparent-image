# remove-image-transparency
Convert transparent pixels of an image to a specific color (white/black) etc.
Also process semi-transparent pixels to smoothen the edges of objects.

## Framework
.Net6-isolated Azure Function V4 app using C#.

## Parameters (using Query-String)
* fillTransparency: white/black/none.
* smoothEdge: true/false.
* blobName: Azure blob path, excluding container name.
* imageUrl: Public Image Url; e.g. https://mysite.com/images/logo.png. App will fetch the image and process it. Either blob name or image url will be processed.

An example url query can be: `http://azure-functionapp.azurebetsites.net/ProcessTransparentImage?fillTransparency=white&smoothEdge=true&blobName=image/log-dark.png`
With imageUrl instead of blobName: `&image-url=https://mycnd.com/images/logo.png`
