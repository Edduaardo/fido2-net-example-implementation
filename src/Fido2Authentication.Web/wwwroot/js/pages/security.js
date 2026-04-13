$(() => {
    // $('#change-password-form').on('submit', (event) => {
    //     if (!event.currentTarget.checkValidity()) {
    //         event.preventDefault()
    //         event.stopPropagation()

    //         $(event.currentTarget).addClass('was-validated')
    //     }
    // })

    // $("#change-password-form").validate({
    //     rules: {
    //         ConfirmNewPassword: {
    //             equalTo: "#newPassword"
    //         }
    //     }
    // });

    const validationSettings = $.data($('#change-password-form')[0], 'validator').settings
    // validationSettings.onfocusout = (element) => {
    //     if (!$('#change-password-form').validate().element(element)) {
    //        $(element).addClass('is-invalid')
    //     } else {
    //        $(element).addClass('is-valid')
    //     }
    // }
    validationSettings.onfocusout = null
    validationSettings.onkeyup = null

    // $.validator.addMethod('equaltocustom', (value, element, params) => {
    //     console.log(params)
    //     // var genre = $(params[0]).val(), year = params[1], date = new Date(value);

    //     // // The Classic genre has a value of '0'.
    //     // if (genre && genre.length > 0 && genre[0] === '0') {
    //     //     // The release date for a Classic is valid if it's no greater than the given year.
    //     //     return date.getUTCFullYear() <= year;
    //     // }
    //     return false
    //     return true;
    // })

    // $.validator.unobtrusive.adapters.add('equaltocustom', ['year'], (options) => {
    //     console.log(options)
    //     var element = $(options.form).find('input#confimNewPassword')[0];

    //     options.rules['equalto'] = [element, options.params['year']];
    //     options.messages['equalto'] = options.message;
    // })

    $('#change-password-form').bind('invalid-form.validate', () => {
        $('#change-password-form').addClass('was-validated')
    })

    $('#user-passkeys-table').DataTable({
        columns: [
            { data: 'name' },
            {
                data: 'creationDate',
                render: (value) => luxon
                    .DateTime
                    .fromISO(value)
                    .toLocaleString(luxon.DateTime.DATETIME_SHORT)
            },
            {
                data: 'id',
                render: () => `<button type="button" title="Edit passkey name" class="btn btn-info text-white" onclick="editPasskey(this)"><i class="bi bi-pen"></i></button>
                <button type="button" title="Remove" class="btn btn-danger" onclick="deletePasskey(this)"><i class="bi bi-trash"></i></button>`
            }
        ],
        ajax: '/get-user-passkeys'
    })

    $('#add-passkey').on('click', async () => {
        const {
            value: passkeyName,
            isConfirmed
        } = await getPasskeyName()

        if (!isConfirmed) return

        let attestationOptions
        try {
            attestationOptions = await fetchAttestationOptions()
        } catch (_) {
            showToastError('An error occurred while trying to get options from the server, try again later.')
            return
        }

        // New:
        attestationOptions = PublicKeyCredential.parseCreationOptionsFromJSON(attestationOptions)

        // Old:
        // Turn the challenge back into the accepted format of padded base64
        // attestationOptions.challenge = coerceToArrayBuffer(attestationOptions.challenge)
        // attestationOptions.user.id = coerceToArrayBuffer(attestationOptions.user.id)
        // attestationOptions.excludeCredentials = attestationOptions.excludeCredentials.forEach(excludedCredential => {
        //     excludedCredential.id = coerceToArrayBuffer(excludedCredential.id)
        // })

        let credentials
        try {
            credentials = await navigator.credentials.create({
                publicKey: attestationOptions
            })
        } catch (_) {
            showToastError('An error ocurred while creating credentials, try again.')
            return
        }

        credentials.name = passkeyName
        await savePasskey(credentials)
    })
})

function getPasskeyName() {
    return Swal.fire({
        title: 'Give a name for this new Passkey',
        input: 'text',
        showCloseButton: true,
        showCancelButton: true,
        inputValidator: (value) => {
            if (!value) return 'Please give your passkey a name';
        }
    })
}

function fetchAttestationOptions() {
    return $.ajax({
        url: '/passkey-get-attestation-options',
        method: 'GET'
    })
}

function savePasskey(credentials) {
    return $.ajax({
        url: `/save-passkey/${credentials.name}`,
        method: 'POST',
        // New:
        data: JSON.stringify(credentials.toJSON()),
        // Old:
        // data: JSON.stringify({
        //     id: credentials.id,
        //     rawId: coerceToBase64Url(new Uint8Array(credentials.rawId)),
        //     type: credentials.type,
        //     extensions: credentials.getClientExtensionResults(),
        //     response: {
        //         attestationObject: coerceToBase64Url(new Uint8Array(credentials.response.attestationObject)),
        //         clientDataJSON: coerceToBase64Url(new Uint8Array(credentials.response.clientDataJSON)),
        //         transports: credentials.response.getTransports()
        //     }
        // }),
        dataType: 'json',
        contentType: 'application/json'
    }).done(() => {
        $('#user-passkeys-table').DataTable().ajax.reload()
        showToastSuccess('Passkey saved successfully.')
    }).catch(() => {
        showToastError('An error occurred while trying to save the passkey, try again later.')
    })
}

function deletePasskey(event) {
    Swal.fire({
        title: 'Are you sure?',
        text: 'You won\'t be able to revert this!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it'
    }).then(result => {
        let passkeysDatatable

        if (result.isConfirmed) {
            passkeysDatatable = $('#user-passkeys-table').DataTable()
            
            $.ajax({
                url: '/delete-user-passkey',
                method: 'DELETE',
                data: {
                    passkeyId: passkeysDatatable.row($(event).closest('tr')).data().id
                }
            }).then(() => {
                passkeysDatatable.ajax.reload()
                showToastSuccess('Passkey deleted.')
            })
        }
    }).catch(() => {
        showToastError('An error occurred while trying to delete the passkey, try again later.')
    })
}

async function editPasskey(event) {
    const {
        value: passkeyName,
        isConfirmed
    } = await getPasskeyName()

    if (!isConfirmed) return

    const passkeysDatatable = $('#user-passkeys-table').DataTable()

    $.ajax({
        url: '/edit-passkey-name',
        method: 'PUT',
        data: JSON.stringify({
            id: passkeysDatatable.row($(event).closest('tr')).data().id,
            name: passkeyName
        }),
        dataType: 'json',
        contentType: 'application/json'
    }).done(() => {
        showToastSuccess('Name successfully changed.')
        passkeysDatatable.ajax.reload()
    }).catch(() => {
        showToastError('An error occurred while trying to change the passkey name, try again later.')
    })
}
