function imagePicker(callback) {
    $.ajax({
        url: imageListService,
        type: 'GET',
        success: function (data) {
            var content = $('#imagePicker .modal-body');
            content.empty();
            for (var i = 0; i < data.length; ++i) {
                var div = $('<a />').attr('href', '#');
                var img = $('<img />').attr('src', data[i]).css({ 'max-width': '250px', 'max-height': '250px' }).addClass('img-thumbnail');
                div.append(img);
                content.append(div);
                div.on('click', function () {
                    callback($(this).find('img').attr('src'));
                    $('#imagePicker').modal('hide');
                });
            }
            $('#imagePicker').modal('show');
        }
    });
}

$(function () {
    $('input.imgpath').each(function (index, element) {
        var div = $("<div />");

        var buttons = $("<div />").addClass('text-right mt-1 mb-1');

        var btnUpload = $("<label />").addClass('btn btn-sm btn-primary btn-file mr-1 mb-0').text('Envoyer une image');
        var inputFile = $("<input />").attr('type', 'file').attr('accept','.png,.jpg,.jpeg,image/*');
        inputFile.hide();
        btnUpload.append(inputFile);
        buttons.append(btnUpload);

        var btnSelect = $("<a />").addClass('btn btn-sm btn-primary').attr('href','#').text('Choisir une image');
        buttons.append(btnSelect);

        var imageDiv = $("<div />").addClass('text-center text-danger');
        var img = $("<img />").css({ 'max-width': '150px', 'max-height': '150px' }).attr('alt','chemin invalide');
        imageDiv.append(img);

        div.append(buttons);
        div.append(imageDiv);
        div.insertAfter($(element));

        function updatePath() {
            var path = $(element).val();
            if (path) {
                img.show(); img.attr('src', $(element).val());
            } else {
                img.hide();
            }
        } 
        updatePath();

        inputFile.on('change', function () {
            var upimg = $('#upimg');
            var file = inputFile[0].files[0];
            var form = new FormData(upimg[0]);
            form.append('image', file);
            $.ajax({
                url: upimg.attr('action'),
                data: form,
                processData: false,
                contentType: false,
                type: 'POST',
                success: function (data) {
                    $(element).val(data);
                    updatePath();
                }
            });
            inputFile.val(null);
        });

        btnSelect.on('click', function (e) {
            e.preventDefault();
            imagePicker(function (url) {
                $(element).val(url);
                updatePath();

            });
        });

        $(element).on('change', updatePath);
    });
});
