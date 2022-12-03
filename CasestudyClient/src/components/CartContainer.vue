<template>
  <div class="text-center">
    <div class="text-h4 q-mt-md">Cart Content</div>
    <q-avatar class="q-mt-md" size="xl" square>
      <img :src="`/joystick.jpg`" />
    </q-avatar>
    <div class="text-h6 text-positive">{{ state.status }}</div>

    <div
      v-if="state.cart.length > 0"
      style="width: 90vw; margin-left: 5vw; margin-top: 1vh"
    >
      <div>
        <q-scroll-area style="height: 60vh">
          <q-card class="q-ma-md">
            <q-list separator>
              <q-item>
                <q-item-section style="font-weight: bold" class="col-3"
                  >Name</q-item-section
                >
                <q-item-secton style="font-weight: bold" class="col-3"
                  >Quantity</q-item-secton
                >
                <q-item-section style="font-weight: bold" class="col-3"
                  >MSRP</q-item-section
                >
                <q-item-section style="font-weight: bold" class="col-3"
                  >Extended</q-item-section
                >
              </q-item>
              <q-item v-for="item in state.cart" :key="item.id" clickable>
                <q-item-section class="col-3">
                  {{ item.item.productName }}
                </q-item-section>
                <q-item-section class="col-3">
                  {{ item.qty }}
                </q-item-section>
                <q-item-section class="col-3">
                  {{ formatCurrency(item.item.msrp) }}
                </q-item-section>
                <q-item-section class="col-3">
                  {{ formatCurrency(item.qty * item.item.msrp) }}
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section style="font-weight: bold">Sub</q-item-section>
                <q-item-section>
                  {{ formatCurrency(state.extendedTot) }}
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section style="font-weight: bold">Taxes</q-item-section>
                <q-item-section>
                  {{ formatCurrency(0.13 * state.extendedTot) }}
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section style="font-weight: bold">Total</q-item-section>
                <q-item-section style="color: green">
                  {{
                    formatCurrency(state.extendedTot + 0.13 * state.extendedTot)
                  }}
                </q-item-section>
              </q-item>
            </q-list>
          </q-card>
        </q-scroll-area>
        <div class="text-center q-mt-md q-mb-lg">
          <q-btn color="primary" label="Clear Cart" @click="clearCart()" />
          <q-btn
            class="q-mr-sm"
            coolor="primary"
            label="Save Cart"
            :disable="state.cart.length < 1"
            @click="saveCart()"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { reactive, onMounted } from "vue";
import { formatCurrency } from "../utils/formatutils";
import { poster } from "../utils/apiutil";

export default {
  setup() {
    let state = reactive({
      status: "",
      qtyTot: 0,
      msrp: 0,
      extendedTot: 0,
      cart: [],
    });

    onMounted(() => {
      if (sessionStorage.getItem("cart")) {
        state.cart = JSON.parse(sessionStorage.getItem("cart"));
        state.cart.forEach((cartItem) => {
          state.qtyTot += cartItem.item.qty;
          state.msrp += cartItem.item.msrp;
          state.extendedTot += cartItem.item.msrp * cartItem.qty;
        });
      } else {
        state.cart = [];
      }
    });

    const clearCart = () => {
      sessionStorage.removeItem("cart");
      state.cart = [];
      state.status = "cart cleared";
    };

    const saveCart = async () => {
      let customer = JSON.parse(sessionStorage.getItem("customer"));
      let cart = JSON.parse(sessionStorage.getItem("cart"));

      try {
        state.status = "sending cart info to server";
        let cartHelper = { email: customer.email, selections: cart };
        let payload = await poster("cart", cartHelper);

        if (payload.indexOf("not") > 0) {
          state.status = payload;
        } else {
          clearCart();
          state.status = payload;
        }
      } catch (err) {
        console.log(err);
        state.status = `Error add tray: ${err}`;
      }
    };

    return {
      state,
      clearCart,
      formatCurrency,
      saveCart,
    };
  },
};
</script>
