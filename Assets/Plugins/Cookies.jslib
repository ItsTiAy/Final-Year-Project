mergeInto(LibraryManager.library, 
{
    Hello: function () 
    {
        window.alert("Hello, world!");
    },

    SetCookie: function(cname, cvalue) 
    {
        document.cookie = UTF8ToString(cname) + "=" + UTF8ToString(cvalue) + "; expires=Tue, 19 Jan 2038 03:14:07 UTC; path=/";
    },

    GetCookie: function(cname) 
    {
        let name = UTF8ToString(cname) + "=";
        let decodedCookie = decodeURIComponent(document.cookie);
        let ca = decodedCookie.split(';');
        for(let i = 0; i < ca.length; i++) 
        {
            let c = ca[i];
            while (c.charAt(0) == ' ') 
            {
                c = c.substring(1);
            }
            if (c.indexOf(name) == 0) 
            {
                var returnStr = c.substring(name.length, c.length);
                var bufferSize = lengthBytesUTF8(returnStr) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(returnStr, buffer, bufferSize);
                return buffer;
            }
        }
        var returnStr = "";
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    DeleteCookie: function(cname)
    {
        document.cookie = UTF8ToString(cname) + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/";
    }
});