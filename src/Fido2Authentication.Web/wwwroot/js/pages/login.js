$(() => {
    $('#login-with-passkey').on('click', async () => {
        const login = $('#login').val()
        
        if (!$('#login-form').validate().element('#login'))
            return
        
        let assertionOptions
        try {
            assertionOptions = await fetchAssetionOptions(login)
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

        credential.userHandle = login
        
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
        $("#change-password-form").addClass('was-validated')
    })
})

function fetchAssetionOptions(login) {
    return $.ajax({
        url: '/get-passkey-assertion-options',
        data: {
            login: login
        }
    })
}

function assertCredential(credential) {
    return $.ajax({
        url: '/make-passkey-assertion',
        method: 'POST',
        data: JSON.stringify({
            id: credential.id,
            rawId: coerceToBase64Url(new Uint8Array(credential.rawId)),
            type: credential.type,
            extensions: credential.getClientExtensionResults(),
            response: {
                authenticatorData: coerceToBase64Url(new Uint8Array(credential.response.authenticatorData)),
                clientDataJSON: coerceToBase64Url(new Uint8Array(credential.response.clientDataJSON)),
                signature: coerceToBase64Url(new Uint8Array(credential.response.signature)),
                userHandle: coerceToBase64Url(coerceToArrayBuffer(btoa(credential.userHandle)))
            }
        }),
        dataType: 'json',
        contentType: 'application/json'
    })
}
