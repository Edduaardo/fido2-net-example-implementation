// Global Javascript functions

function showToastSuccess(message, messageTitle = null) {
    showToast('success', messageTitle ?? 'Success', message)
}

function showToastError(message, messageTitle = null) {
    showToast('error', messageTitle ?? 'Error', message)
}

function showToastInformation(message, messageTitle = null) {
    showToast('information', messageTitle ?? 'Information', message)
}

function showToastWarning(message, messageTitle = null) {
    showToast('warning', messageTitle ?? 'Warning', message)
}

function showToast(
    messageType,
    messageTitle,
    message) {
        const getHeaderColor = () => {
            const types = {
                success: 'bg-success',
                error: 'bg-danger',
                information: 'bg-primary',
                warning: 'bg-warning'
            }
            
            return types[messageType]
        }

        $('main').find('.toast-container').remove()
        $('main').append(
            `<div class="toast-container position-fixed bottom-0 end-0 p-3">
            <div id="live-toast" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="10000">
                <div class="toast-header ${getHeaderColor()} text-white" data-bs-theme="dark">
                    <strong class="me-auto"> ${messageTitle}</strong>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        </div>`)

        bootstrap.Toast.getOrCreateInstance(
            document.getElementById('live-toast')
        ).show()
}

function showLoading() {
    
}
