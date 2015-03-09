/*!
 * DbWebApi Client JavaScript Library v1.0.1-alpha
 * https://github.com/databooster/dbwebapi
 * Date: 2015-03-09
*/
(function (window, undefined) {
	jQuery.support.cors = true;

	jQuery.extend({
		postDb: function (url, inputJson, successCallback, errorCallback) {
			var jsonContent;

			if (jQuery.isPlainObject(inputJson))
				jsonContent = JSON.stringify(inputJson);
			else if (typeof inputJson === "string")
				jsonContent = inputJson;
			else
				jsonContent = "{}";

			return jQuery.ajax({
				type: "POST",
				url: url,
				contentType: "application/json;charset=utf-8",
				data: jsonContent,
				processData: false,
				xhrFields: { withCredentials: true },
				success: successCallback,
				error: errorCallback
			});
		}
	});
})(window);
