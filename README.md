# process-transparent-image
Convert transparent pixels of an image to a specific color e.g. white/black etc. Also process semi-transparent pixels for smooth edges of texts or other objects. This is .Net8-isolated Azure Function V4 app using C#.

## QueryString parameters
* `fill-transparency`: white/black/none.
* `smooth-edging`: true/false. Default: true.
* `image-url`: Image Url; e.g. `https://mysite.com/images/logo.png`. App will fetch the image and process it. 

So, and example query can be: *?**fill-transparency**=white&**smooth-edging**=true&**image-url**=https://mysite.com/images/logo.png*
