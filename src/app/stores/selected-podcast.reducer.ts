export function selectedPodcastReducer(state: any = null, {type, payload}) {
    switch (type) {
        case 'SELECT_ITEM':
            return payload;
        default:
            return state;
    }
}