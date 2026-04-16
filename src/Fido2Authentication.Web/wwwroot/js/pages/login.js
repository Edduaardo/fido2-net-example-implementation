$(() => {
    $('#login-with-passkey').on('click', async () => {
        const login = $('#login')
        
        if (login.val() && !$('#login-form').validate().element('#login')) {
            $('#login').addClass('is-invalid')
            return
        } else {
            $('#login').removeClass('is-invalid')
        }
        
        let assertionOptions
        try {
            assertionOptions = await fetchAssetionOptions(login.val())
        } catch (_) {
            showToastError('An error occurred while trying to get options from the server, please try again later.')
            return
        }

        // New:
        assertionOptions = PublicKeyCredential.parseRequestOptionsFromJSON(assertionOptions)
        // Old:
        // assertionOptions.challenge = coerceToArrayBuffer(assertionOptions.challenge)
        // assertionOptions.allowCredentials.forEach(listItem => {
        //     listItem.id = coerceToArrayBuffer(listItem.id)
        // })

        let credential
        try {
            credential = await navigator.credentials.get({
                publicKey: assertionOptions
            })
        } catch (_) {
            showToastError('An error occurred while getting credentials, please try again.')
            return
        }

        let assertionResult
        try {
            assertionResult = await assertCredential(credential)
        } catch(_) {
            showToastError('An error occurred while verifying your credentials, please try again.')
            return
        }

        window.location = '/Home'
    })

    const validationSettings = $.data($('#login-form')[0], 'validator').settings
    validationSettings.onfocusout = null
    validationSettings.onkeyup = null

    $('#login-form').bind('invalid-form.validate', () => {
        $("#login-form").addClass('was-validated')
    })
})

async function fetchAssetionOptions(login) {
    return $.ajax({
        url: '/get-passkey-assertion-options',
        method: 'GET',
        data: {
            login: login
        }
    })
}

function assertCredential(credential) {
    return $.ajax({
        url: '/make-passkey-assertion',
        method: 'POST',
        data: JSON.stringify(credential), // credentials.toJSON() is called explicity
        dataType: 'json',
        contentType: 'application/json'
    })
}
