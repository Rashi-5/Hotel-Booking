function showEditBookingForm(bookingId) {
    // Step 1: Load the partial view into the container
    fetch('/User/EditBookingPartial?bookingId=' + bookingId)
        .then(response => response.text())
        .then(html => {
            document.getElementById('editBookingFormContainer').innerHTML = html;
            document.getElementById('bookingFormContainer').style.display = 'block';
            document.getElementById('bookingFormContainer').scrollIntoView({ behavior: 'smooth' });

            fetch('/User/GetBookingData?bookingId=' + bookingId)
                .then(res => res.json())
                .then(data => {
                    setTimeout(() => populateEditBookingForm(data), 50);
                });
        });
}
